
using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Models;

namespace Skedula.Api.Services;

public interface IFatigueService
{
    Task UpdateFatigueMetricsAsync(int employeeId);
    Task<double> GetFatigueScoreAsync(int employeeId);
    Task<List<int>> GetEligibleEmployeeIdsAsync(DateTime date, string shiftCode);
}

public class FatigueService : IFatigueService
{
    private readonly SchedulingDbContext _context;

    public FatigueService(SchedulingDbContext context)
    {
        _context = context;
    }

    public async Task UpdateFatigueMetricsAsync(int employeeId)
    {
        var today = DateTime.Today;
        var fourteenDaysAgo = today.AddDays(-14);
        var twentyEightDaysAgo = today.AddDays(-28);

        var recentSchedules = await _context.Schedules
            .Include(s => s.ShiftType)
            .Where(s => s.EmployeeId == employeeId && s.Date >= twentyEightDaysAgo && s.Date <= today)
            .OrderBy(s => s.Date)
            .ToListAsync();

        var metric = await _context.EmployeeFatigueMetrics
            .FirstOrDefaultAsync(m => m.EmployeeId == employeeId);

        if (metric == null)
        {
            metric = new EmployeeFatigueMetric { EmployeeId = employeeId };
            _context.EmployeeFatigueMetrics.Add(metric);
        }

        // Calculate consecutive work days
        var consecutiveDays = 0;
        for (var d = today; d >= today.AddDays(-30); d = d.AddDays(-1))
        {
            var schedule = recentSchedules.FirstOrDefault(s => s.Date.Date == d.Date);
            if (schedule != null && !schedule.ShiftType.IsRestDay)
            {
                consecutiveDays++;
            }
            else
            {
                break;
            }
        }

        // Work days in last 14 days
        var workDaysIn14 = recentSchedules
            .Where(s => s.Date >= fourteenDaysAgo && !s.ShiftType.IsRestDay)
            .Count();

        // Weekend shifts in last 4 weeks
        var weekendShifts = recentSchedules
            .Where(s => (s.Date.DayOfWeek == DayOfWeek.Saturday || s.Date.DayOfWeek == DayOfWeek.Sunday)
                        && !s.ShiftType.IsRestDay)
            .Count();

        // Last CO date
        var lastCO = recentSchedules
            .Where(s => s.ShiftType.Code == "CO")
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();

        // Last E date
        var lastE = recentSchedules
            .Where(s => s.ShiftType.Code == "E")
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();

        // Last C on weekend
        var lastCWeekend = recentSchedules
            .Where(s => s.ShiftType.Code == "C" && 
                       (s.Date.DayOfWeek == DayOfWeek.Saturday || s.Date.DayOfWeek == DayOfWeek.Sunday))
            .OrderByDescending(s => s.Date)
            .FirstOrDefault();

        metric.ConsecutiveWorkDays = consecutiveDays;
        metric.TotalWorkDaysInPeriod = workDaysIn14;
        metric.WeekendShiftsCount = weekendShifts;
        metric.LastCODate = lastCO?.Date;
        metric.LastEDate = lastE?.Date;
        metric.LastCOnWeekendDate = lastCWeekend?.Date;
        metric.WeekStartDate = today.AddDays(-(int)today.DayOfWeek);
        metric.LastUpdated = DateTime.UtcNow;

        // Calculate fatigue score
        var daysSinceLastCO = lastCO != null ? (today - lastCO.Date).Days : 14;
        
        metric.FatigueScore = 
            (consecutiveDays * 2.0) +
            (workDaysIn14 / 14.0 * 10) +
            (weekendShifts * 1.5) +
            (daysSinceLastCO < 7 ? 0 : (daysSinceLastCO - 7) * 0.5);

        await _context.SaveChangesAsync();
    }

    public async Task<double> GetFatigueScoreAsync(int employeeId)
    {
        var metric = await _context.EmployeeFatigueMetrics
            .FirstOrDefaultAsync(m => m.EmployeeId == employeeId);

        if (metric == null || (DateTime.UtcNow - metric.LastUpdated).TotalHours > 24)
        {
            await UpdateFatigueMetricsAsync(employeeId);
            metric = await _context.EmployeeFatigueMetrics
                .FirstOrDefaultAsync(m => m.EmployeeId == employeeId);
        }

        return metric?.FatigueScore ?? 0;
    }

    public async Task<List<int>> GetEligibleEmployeeIdsAsync(DateTime date, string shiftCode)
    {
        var allEmployees = await _context.Employees
            .Where(e => e.IsActive)
            .Select(e => e.EmployeeId)
            .ToListAsync();

        var eligibleIds = new List<int>();

        foreach (var empId in allEmployees)
        {
            // Check if already assigned on this date
            var existingSchedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.EmployeeId == empId && s.Date.Date == date.Date);

            if (existingSchedule != null)
                continue;

            // Check if on leave
            var onLeave = await _context.LeaveRequests
                .AnyAsync(lr => lr.EmployeeId == empId && 
                               lr.Status == "Approved" &&
                               lr.StartDate.Date <= date.Date && 
                               lr.EndDate.Date >= date.Date);

            if (onLeave)
                continue;

            // Check consecutive work days
            var employee = await _context.Employees.FindAsync(empId);
            var metric = await _context.EmployeeFatigueMetrics
                .FirstOrDefaultAsync(m => m.EmployeeId == empId);

            if (metric != null && metric.ConsecutiveWorkDays >= employee.MaxConsecutiveWorkDays)
                continue;

            eligibleIds.Add(empId);
        }

        return eligibleIds;
    }
}