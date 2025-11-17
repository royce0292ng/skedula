using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Models;

namespace Skedula.Api.Services;

public interface IConstraintService
{
    Task<bool> ValidateHardConstraintsAsync(List<Schedule> schedules);
    Task<double> CalculateAssignmentScoreAsync(int employeeId, DateTime date, string shiftCode);
}

public class ConstraintService : IConstraintService
{
    private readonly SchedulingDbContext _context;
    private readonly IFatigueService _fatigueService;

    public ConstraintService(SchedulingDbContext context, IFatigueService fatigueService)
    {
        _context = context;
        _fatigueService = fatigueService;
    }

    public async Task<bool> ValidateHardConstraintsAsync(List<Schedule> schedules)
    {
        // Group by date
        var dateGroups = schedules.GroupBy(s => s.Date.Date);

        foreach (var dateGroup in dateGroups)
        {
            var date = dateGroup.Key;
            var daySchedules = await _context.Schedules
                .Include(s => s.ShiftType)
                .Where(s => s.Date.Date == date)
                .ToListAsync();

            // Check A shift: exactly 2
            var aCount = daySchedules.Count(s => s.ShiftType.Code == "A");
            if (aCount != 2)
                return false;

            // Check C shift: exactly 3
            var cCount = daySchedules.Count(s => s.ShiftType.Code == "C");
            if (cCount != 3)
                return false;

            // Check E shift: exactly 3
            var eCount = daySchedules.Count(s => s.ShiftType.Code == "E");
            if (eCount != 3)
                return false;

            // Check no double assignments
            var employeeIds = daySchedules.Select(s => s.EmployeeId).ToList();
            if (employeeIds.Count != employeeIds.Distinct().Count())
                return false;
        }

        return true;
    }

    public async Task<double> CalculateAssignmentScoreAsync(int employeeId, DateTime date, string shiftCode)
    {
        double score = 100.0; // Base score

        // Get fatigue metrics
        var fatigueScore = await _fatigueService.GetFatigueScoreAsync(employeeId);
        score -= (fatigueScore * 10);

        var metric = await _context.EmployeeFatigueMetrics
            .FirstOrDefaultAsync(m => m.EmployeeId == employeeId);

        if (metric != null)
        {
            // Penalize consecutive work days
            score -= (metric.ConsecutiveWorkDays * 5);

            // Penalize weekend shifts
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                score -= (metric.WeekendShiftsCount * 3);
            }

            // Reward if due for CO
            var daysSinceLastCO = metric.LastCODate.HasValue 
                ? (date - metric.LastCODate.Value).Days 
                : 14;
            
            if (shiftCode == "CO" && daysSinceLastCO >= 14)
            {
                score += 20;
            }

            // Reward if due for E after weekend C
            if (shiftCode == "E" && date.DayOfWeek == DayOfWeek.Monday)
            {
                var lastWeekend = date.AddDays(-2); // Saturday
                var hadCOnWeekend = await _context.Schedules
                    .Include(s => s.ShiftType)
                    .AnyAsync(s => s.EmployeeId == employeeId && 
                                  s.ShiftType.Code == "C" &&
                                  (s.Date.Date == lastWeekend.Date || s.Date.Date == lastWeekend.AddDays(1).Date));
                
                if (hadCOnWeekend)
                {
                    score += 30; // High priority
                }
            }

            // Incentivize if haven't had this shift recently
            var lastSameShift = await _context.Schedules
                .Include(s => s.ShiftType)
                .Where(s => s.EmployeeId == employeeId && s.ShiftType.Code == shiftCode)
                .OrderByDescending(s => s.Date)
                .FirstOrDefaultAsync();

            if (lastSameShift != null)
            {
                var daysSinceLastSame = (date - lastSameShift.Date).Days;
                score += (daysSinceLastSame * 0.5);
            }
        }

        return score;
    }
}