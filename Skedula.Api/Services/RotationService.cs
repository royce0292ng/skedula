using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Models;

namespace Skedula.Api.Services;

public interface IRotationService
{
    Task<int?> GetNextInRotationAsync(string rotationType, int shiftTypeId);
    Task UpdateRotationAsync(string rotationType, int shiftTypeId, int employeeId, DateTime date);
    Task InitializeRotationsAsync();
}

public class RotationService : IRotationService
{
    private readonly SchedulingDbContext _context;

    public RotationService(SchedulingDbContext context)
    {
        _context = context;
    }

    public async Task InitializeRotationsAsync()
    {
        var allEmployeeIds = await _context.Employees
            .Where(e => e.IsActive)
            .Select(e => e.EmployeeId)
            .ToListAsync();

        var rotationTypes = new[] { "MondayCO", "SaturdayEC", "SaturdayE" };

        foreach (var rotationType in rotationTypes)
        {
            var exists = await _context.ShiftRotationTrackers
                .AnyAsync(r => r.RotationType == rotationType);

            if (!exists)
            {
                var shiftTypeId = rotationType switch
                {
                    "MondayCO" => 5, // CO
                    "SaturdayEC" => 6, // EC
                    "SaturdayE" => 4, // E
                    _ => 0
                };

                var tracker = new ShiftRotationTracker
                {
                    ShiftTypeId = shiftTypeId,
                    RotationType = rotationType,
                    RotationSequence = JsonSerializer.Serialize(allEmployeeIds),
                    CurrentIndex = 0
                };

                _context.ShiftRotationTrackers.Add(tracker);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetNextInRotationAsync(string rotationType, int shiftTypeId)
    {
        var tracker = await _context.ShiftRotationTrackers
            .FirstOrDefaultAsync(r => r.RotationType == rotationType);

        if (tracker == null)
            return null;

        var sequence = JsonSerializer.Deserialize<List<int>>(tracker.RotationSequence);
        
        if (sequence == null || sequence.Count == 0)
            return null;

        var nextEmployeeId = sequence[tracker.CurrentIndex];
        
        return nextEmployeeId;
    }

    public async Task UpdateRotationAsync(string rotationType, int shiftTypeId, int employeeId, DateTime date)
    {
        var tracker = await _context.ShiftRotationTrackers
            .FirstOrDefaultAsync(r => r.RotationType == rotationType);

        if (tracker == null)
            return;

        var sequence = JsonSerializer.Deserialize<List<int>>(tracker.RotationSequence);
        
        tracker.LastAssignedEmployeeId = employeeId;
        tracker.LastAssignedDate = date;
        tracker.CurrentIndex = (tracker.CurrentIndex + 1) % sequence.Count;

        await _context.SaveChangesAsync();
    }
}