using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Models;

namespace Skedula.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly SchedulingDbContext _context;

    public LeaveController(SchedulingDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLeaves([FromQuery] string status = null)
    {
        var query = _context.LeaveRequests.Include(lr => lr.Employee).AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(lr => lr.Status == status);
        }

        var leaves = await query.OrderByDescending(lr => lr.RequestedAt).ToListAsync();
        return Ok(leaves);
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeLeaves(int employeeId)
    {
        var leaves = await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.RequestedAt)
            .ToListAsync();

        return Ok(leaves);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequest request)
    {
        var leave = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            LeaveType = request.LeaveType,
            Status = "Pending"
        };

        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployeeLeaves), new { employeeId = request.EmployeeId }, leave);
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        
        if (leave == null)
            return NotFound();

        leave.Status = "Approved";
        await _context.SaveChangesAsync();

        return Ok(leave);
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectLeave(int id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        
        if (leave == null)
            return NotFound();

        leave.Status = "Rejected";
        await _context.SaveChangesAsync();

        return Ok(leave);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLeave(int id)
    {
        var leave = await _context.LeaveRequests.FindAsync(id);
        
        if (leave == null)
            return NotFound();

        _context.LeaveRequests.Remove(leave);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }
}

public class CreateLeaveRequest
{
    public int EmployeeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeaveType { get; set; }
}