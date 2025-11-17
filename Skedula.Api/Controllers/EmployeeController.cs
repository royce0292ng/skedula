using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Models;
using Skedula.Api.Services;

namespace Skedula.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly SchedulingDbContext _context;
    private readonly IFatigueService _fatigueService;

    public EmployeeController(SchedulingDbContext context, IFatigueService fatigueService)
    {
        _context = context;
        _fatigueService = fatigueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        var employees = await _context.Employees
            .Where(e => e.IsActive)
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
    {
        var employee = new Employee
        {
            Name = request.Name,
            EmployeeCode = request.EmployeeCode,
            DateJoined = request.DateJoined,
            IsActive = true,
            MaxConsecutiveWorkDays = request.MaxConsecutiveWorkDays ?? 6
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Initialize fatigue metrics
        await _fatigueService.UpdateFatigueMetricsAsync(employee.EmployeeId);

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var employee = await _context.Employees.FindAsync(id);
        
        if (employee == null)
            return NotFound();

        employee.Name = request.Name ?? employee.Name;
        employee.EmployeeCode = request.EmployeeCode ?? employee.EmployeeCode;
        employee.MaxConsecutiveWorkDays = request.MaxConsecutiveWorkDays ?? employee.MaxConsecutiveWorkDays;
        employee.IsActive = request.IsActive ?? employee.IsActive;

        await _context.SaveChangesAsync();

        return Ok(employee);
    }

    [HttpGet("{id}/fatigue")]
    public async Task<IActionResult> GetEmployeeFatigue(int id)
    {
        await _fatigueService.UpdateFatigueMetricsAsync(id);
        
        var metric = await _context.EmployeeFatigueMetrics
            .FirstOrDefaultAsync(m => m.EmployeeId == id);

        if (metric == null)
            return NotFound();

        return Ok(metric);
    }
}

public class CreateEmployeeRequest
{
    public string Name { get; set; }
    public string EmployeeCode { get; set; }
    public DateTime DateJoined { get; set; }
    public int? MaxConsecutiveWorkDays { get; set; }
}

public class UpdateEmployeeRequest
{
    public string Name { get; set; }
    public string EmployeeCode { get; set; }
    public int? MaxConsecutiveWorkDays { get; set; }
    public bool? IsActive { get; set; }
}
