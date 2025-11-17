using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Models;

namespace Skedula.Api.Services;

public interface ISchedulingService
{
    Task<List<Schedule>> GenerateScheduleAsync(DateTime startDate, DateTime endDate);
    Task<bool> ValidateScheduleAsync(DateTime date);
}

public class SchedulingService : ISchedulingService
{
    private readonly SchedulingDbContext _context;
    private readonly IFatigueService _fatigueService;
    private readonly IConstraintService _constraintService;
    private readonly IRotationService _rotationService;

    public SchedulingService(
        SchedulingDbContext context,
        IFatigueService fatigueService,
        IConstraintService constraintService,
        IRotationService rotationService)
    {
        _context = context;
        _fatigueService = fatigueService;
        _constraintService = constraintService;
        _rotationService = rotationService;
    }

    public async Task<List<Schedule>> GenerateScheduleAsync(DateTime startDate, DateTime endDate)
    {
        var generatedSchedules = new List<Schedule>();

        // Phase 1: Update all fatigue metrics
        var allEmployees = await _context.Employees
            .Where(e => e.IsActive)
            .ToListAsync();

        foreach (var emp in allEmployees)
        {
            await _fatigueService.UpdateFatigueMetricsAsync(emp.EmployeeId);
        }

        // Phase 2: Process each day
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            await ProcessDayAsync(date, generatedSchedules);
        }

        return generatedSchedules;
    }

    private async Task ProcessDayAsync(DateTime date, List<Schedule> generatedSchedules)
    {
        // Step 1: Assign approved leaves
        var leavesForDay = await _context.LeaveRequests
            .Where(lr => lr.Status == "Approved" && 
                        lr.StartDate.Date <= date.Date && 
                        lr.EndDate.Date >= date.Date)
            .ToListAsync();

        foreach (var leave in leavesForDay)
        {
            var schedule = new Schedule
            {
                EmployeeId = leave.EmployeeId,
                Date = date,
                ShiftTypeId = 7, // AL
                IsAutoAssigned = true,
                Notes = "Annual Leave"
            };
            
            _context.Schedules.Add(schedule);
            generatedSchedules.Add(schedule);
        }

        // Step 2: Handle CO assignments (every 14 days + rotations)
        await AssignCOShiftsAsync(date, generatedSchedules);

        // Step 3: Handle weekend C → Monday E rule
        if (date.DayOfWeek == DayOfWeek.Monday)
        {
            await AssignMondayEAfterWeekendCAsync(date, generatedSchedules);
        }

        // Step 4: Handle rotations
        await AssignRotationsAsync(date, generatedSchedules);

        // Step 5: Assign A, C, E, D shifts with scoring
        await AssignRegularShiftsAsync(date, generatedSchedules);

        await _context.SaveChangesAsync();
    }

    private async Task AssignCOShiftsAsync(DateTime date, List<Schedule> generatedSchedules)
    {
        var allEmployees = await _context.Employees
            .Where(e => e.IsActive)
            .ToListAsync();

        foreach (var emp in allEmployees)
        {
            var metric = await _context.EmployeeFatigueMetrics
                .FirstOrDefaultAsync(m => m.EmployeeId == emp.EmployeeId);

            if (metric?.LastCODate == null || (date - metric.LastCODate.Value).Days >= 14)
            {
                // Check if not already assigned
                var alreadyAssigned = generatedSchedules.Any(s => s.EmployeeId == emp.EmployeeId && s.Date.Date == date.Date);
                var existingSchedule = await _context.Schedules
                    .AnyAsync(s => s.EmployeeId == emp.EmployeeId && s.Date.Date == date.Date);

                if (!alreadyAssigned && !existingSchedule)
                {
                    var schedule = new Schedule
                    {
                        EmployeeId = emp.EmployeeId,
                        Date = date,
                        ShiftTypeId = 5, // CO
                        IsAutoAssigned = true,
                        Notes = "14-day CO cycle"
                    };

                    _context.Schedules.Add(schedule);
                    generatedSchedules.Add(schedule);
                    
                    await _fatigueService.UpdateFatigueMetricsAsync(emp.EmployeeId);
                    
                    break; // Only one CO per consideration
                }
            }
        }
    }

    private async Task AssignMondayEAfterWeekendCAsync(DateTime monday, List<Schedule> generatedSchedules)
    {
        var saturday = monday.AddDays(-2);
        var sunday = monday.AddDays(-1);

        // Find who worked C on weekend
        var weekendCEmployees = await _context.Schedules
            .Include(s => s.ShiftType)
            .Where(s => (s.Date.Date == saturday.Date || s.Date.Date == sunday.Date) && 
                       s.ShiftType.Code == "C")
            .Select(s => s.EmployeeId)
            .Distinct()
            .ToListAsync();

        // Try to assign them E on Monday
        foreach (var empId in weekendCEmployees)
        {
            var alreadyAssigned = generatedSchedules.Any(s => s.EmployeeId == empId && s.Date.Date == monday.Date);
            var existingSchedule = await _context.Schedules
                .AnyAsync(s => s.EmployeeId == empId && s.Date.Date == monday.Date);

            if (!alreadyAssigned && !existingSchedule)
            {
                var eShiftType = await _context.ShiftTypes.FirstOrDefaultAsync(st => st.Code == "E");
                
                var schedule = new Schedule
                {
                    EmployeeId = empId,
                    Date = monday,
                    ShiftTypeId = eShiftType.ShiftTypeId,
                    IsAutoAssigned = true,
                    Notes = "E after weekend C"
                };

                _context.Schedules.Add(schedule);
                generatedSchedules.Add(schedule);
            }
        }
    }

    private async Task AssignRotationsAsync(DateTime date, List<Schedule> generatedSchedules)
    {
        // Monday CO rotation
        if (date.DayOfWeek == DayOfWeek.Monday)
        {
            var nextEmpId = await _rotationService.GetNextInRotationAsync("MondayCO", 5);
            if (nextEmpId.HasValue)
            {
                var alreadyAssigned = generatedSchedules.Any(s => s.EmployeeId == nextEmpId.Value && s.Date.Date == date.Date);
                if (!alreadyAssigned)
                {
                    var schedule = new Schedule
                    {
                        EmployeeId = nextEmpId.Value,
                        Date = date,
                        ShiftTypeId = 5,
                        IsAutoAssigned = true,
                        Notes = "Monday CO rotation"
                    };
                    
                    _context.Schedules.Add(schedule);
                    generatedSchedules.Add(schedule);
                    
                    await _rotationService.UpdateRotationAsync("MondayCO", 5, nextEmpId.Value, date);
                }
            }
        }

        // Saturday EC and E rotations
        if (date.DayOfWeek == DayOfWeek.Saturday)
        {
            var nextECEmpId = await _rotationService.GetNextInRotationAsync("SaturdayEC", 6);
            if (nextECEmpId.HasValue)
            {
                var alreadyAssigned = generatedSchedules.Any(s => s.EmployeeId == nextECEmpId.Value && s.Date.Date == date.Date);
                if (!alreadyAssigned)
                {
                    var schedule = new Schedule
                    {
                        EmployeeId = nextECEmpId.Value,
                        Date = date,
                        ShiftTypeId = 6,
                        IsAutoAssigned = true,
                        Notes = "Saturday EC rotation"
                    };
                    
                    _context.Schedules.Add(schedule);
                    generatedSchedules.Add(schedule);
                    
                    await _rotationService.UpdateRotationAsync("SaturdayEC", 6, nextECEmpId.Value, date);
                }
            }
        }
    }

    private async Task AssignRegularShiftsAsync(DateTime date, List<Schedule> generatedSchedules)
    {
        var shiftTypes = await _context.ShiftTypes
            .Where(st => st.Code == "A" || st.Code == "C" || st.Code == "E")
            .OrderBy(st => st.Priority)
            .ToListAsync();

        foreach (var shiftType in shiftTypes)
        {
            var currentAssignedCount = generatedSchedules
                .Count(s => s.Date.Date == date.Date && s.ShiftTypeId == shiftType.ShiftTypeId);

            var existingCount = await _context.Schedules
                .CountAsync(s => s.Date.Date == date.Date && s.ShiftTypeId == shiftType.ShiftTypeId);

            var totalAssigned = currentAssignedCount + existingCount;
            var needed = shiftType.RequiredHeadcount - totalAssigned;

            if (needed <= 0)
                continue;

            // Get eligible employees
            var eligibleIds = await _fatigueService.GetEligibleEmployeeIdsAsync(date, shiftType.Code);

            // Remove already assigned employees
            var assignedIds = generatedSchedules
                .Where(s => s.Date.Date == date.Date)
                .Select(s => s.EmployeeId)
                .ToList();

            var existingAssignedIds = await _context.Schedules
                .Where(s => s.Date.Date == date.Date)
                .Select(s => s.EmployeeId)
                .ToListAsync();

            eligibleIds = eligibleIds
                .Except(assignedIds)
                .Except(existingAssignedIds)
                .ToList();

            // Score and sort employees
            var scoredEmployees = new List<(int EmployeeId, double Score)>();

            foreach (var empId in eligibleIds)
            {
                var score = await _constraintService.CalculateAssignmentScoreAsync(empId, date, shiftType.Code);
                scoredEmployees.Add((empId, score));
            }

            var topEmployees = scoredEmployees
                .OrderByDescending(e => e.Score)
                .Take(needed)
                .ToList();

            // Assign shifts
            foreach (var emp in topEmployees)
            {
                var schedule = new Schedule
                {
                    EmployeeId = emp.EmployeeId,
                    Date = date,
                    ShiftTypeId = shiftType.ShiftTypeId,
                    IsAutoAssigned = true,
                    Notes = $"Auto-assigned based on score: {emp.Score:F2}"
                };

                _context.Schedules.Add(schedule);
                generatedSchedules.Add(schedule);

                await _fatigueService.UpdateFatigueMetricsAsync(emp.EmployeeId);
            }
        }

        // Assign remaining employees to D shift
        var allActiveEmployees = await _context.Employees
            .Where(e => e.IsActive)
            .Select(e => e.EmployeeId)
            .ToListAsync();

        var allAssignedIds = generatedSchedules
            .Where(s => s.Date.Date == date.Date)
            .Select(s => s.EmployeeId)
            .ToList();

        var existingAllAssignedIds = await _context.Schedules
            .Where(s => s.Date.Date == date.Date)
            .Select(s => s.EmployeeId)
            .ToListAsync();

        var unassignedIds = allActiveEmployees
            .Except(allAssignedIds)
            .Except(existingAllAssignedIds)
            .ToList();

        var dShiftType = await _context.ShiftTypes.FirstOrDefaultAsync(st => st.Code == "D");

        foreach (var empId in unassignedIds)
        {
            var schedule = new Schedule
            {
                EmployeeId = empId,
                Date = date,
                ShiftTypeId = dShiftType.ShiftTypeId,
                IsAutoAssigned = true,
                Notes = "Default D shift assignment"
            };

            _context.Schedules.Add(schedule);
            generatedSchedules.Add(schedule);
        }
    }

    public async Task<bool> ValidateScheduleAsync(DateTime date)
    {
        var schedules = await _context.Schedules
            .Where(s => s.Date.Date == date.Date)
            .ToListAsync();

        return await _constraintService.ValidateHardConstraintsAsync(schedules);
    }
}