using Microsoft.AspNetCore.Mvc;
using Skedula.Api.Models;
using Skedula.Api.Services;

namespace Skedula.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulingController : ControllerBase
{
 
    private readonly SchedulingService _schedulingService = new();

    [HttpGet("generate")]
    public ActionResult<IEnumerable<ScheduleDay>> GenerateSchedule(int year, int month)
    {
        var employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "Royce", Role = "Barista", Group = "B" },
            new Employee { Id = 2, Name = "Jamie", Role = "Manager", Group = "B" },
            new Employee { Id = 3, Name = "Alex", Role = "Cashier", Group = "A" },
            new Employee { Id = 4, Name = "Taylor", Role = "Barista", Group = "A" },
            new Employee { Id = 5, Name = "Morgan", Role = "Cleaner", Group = "C" },
            new Employee { Id = 6, Name = "Chris", Role = "Supervisor", Group = "C" },
            new Employee { Id = 7, Name = "Jordan", Role = "Kitchen Staff", Group = "A" },
            new Employee { Id = 8, Name = "Sam", Role = "Cashier", Group = "B" },
            new Employee { Id = 9, Name = "Charlie", Role = "Kitchen Staff", Group = "C" },
            new Employee { Id = 10, Name = "Riley", Role = "Barista", Group = "B" },
            new Employee { Id = 11, Name = "Avery", Role = "Cleaner", Group = "A" },
            new Employee { Id = 12, Name = "Dakota", Role = "Barista", Group = "C" },
            new Employee { Id = 13, Name = "Casey", Role = "Manager", Group = "A" },
            new Employee { Id = 14, Name = "Emerson", Role = "Cashier", Group = "B" },
            new Employee { Id = 15, Name = "Harper", Role = "Barista", Group = "C" },
            new Employee { Id = 16, Name = "Peyton", Role = "Kitchen Staff", Group = "B" },
            new Employee { Id = 17, Name = "Skyler", Role = "Barista", Group = "C" },
            new Employee { Id = 18, Name = "Rowan", Role = "Cashier", Group = "A" },
            new Employee { Id = 19, Name = "Jordan", Role = "Manager", Group = "A" },
            new Employee { Id = 20, Name = "Cameron", Role = "Kitchen Staff", Group = "B" }
        };


        var schedule = _schedulingService.GenerateMonthlySchedule(employees, year, month);
        return Ok(schedule);
    }
    
}