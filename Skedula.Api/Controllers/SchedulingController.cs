using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Models;
using Skedula.Api.Services;

namespace Skedula.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ISchedulingService _schedulingService;
    private readonly SchedulingDbContext _context;

    public ScheduleController(ISchedulingService schedulingService, SchedulingDbContext context)
    {
        _schedulingService = schedulingService;
        _context = context;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateSchedule([FromBody] GenerateScheduleRequest request)
    {
        try
        {
            var schedules = await _schedulingService.GenerateScheduleAsync(request.StartDate, request.EndDate);
            return Ok(new { success = true, count = schedules.Count, schedules });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("range")]
    public async Task<IActionResult> GetScheduleRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var schedules = await _context.Schedules
            .Include(s => s.Employee)
            .Include(s => s.ShiftType)
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Employee.Name)
            .ToListAsync();

        return Ok(schedules);
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeSchedule(int employeeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.Today;
        var end = endDate ?? DateTime.Today.AddDays(30);

        var schedules = await _context.Schedules
            .Include(s => s.ShiftType)
            .Where(s => s.EmployeeId == employeeId && s.Date >= start && s.Date <= end)
            .OrderBy(s => s.Date)
            .ToListAsync();

        return Ok(schedules);
    }

    [HttpPut("{scheduleId}")]
    public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] UpdateScheduleRequest request)
    {
        var schedule = await _context.Schedules.FindAsync(scheduleId);
        
        if (schedule == null)
            return NotFound();

        schedule.ShiftTypeId = request.ShiftTypeId;
        schedule.IsAutoAssigned = false;
        schedule.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return Ok(schedule);
    }

    [HttpDelete("{scheduleId}")]
    public async Task<IActionResult> DeleteSchedule(int scheduleId)
    {
        var schedule = await _context.Schedules.FindAsync(scheduleId);
        
        if (schedule == null)
            return NotFound();

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpGet("validate")]
    public async Task<IActionResult> ValidateSchedule([FromQuery] DateTime date)
    {
        var isValid = await _schedulingService.ValidateScheduleAsync(date);
        return Ok(new { date, isValid });
    }
}

public class GenerateScheduleRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdateScheduleRequest
{
    public int ShiftTypeId { get; set; }
    public string Notes { get; set; }
}