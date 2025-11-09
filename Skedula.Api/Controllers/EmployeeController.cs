using Microsoft.AspNetCore.Mvc;
using Skedula.Api.Models;

namespace Skedula.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class EmployeeController : ControllerBase
    {
        // In-memory "database"
        private static readonly List<Employee> Employees = new()
        {
            new Employee { Id = 1, Name = "Royce", Group = "B", Role = "Manager" },
            new Employee { Id = 2, Name = "Park", Group = "A", Role = "Staff" },
        };

        // GET api/employee
        [HttpGet]
        public ActionResult<IEnumerable<Employee>> GetAll()
        {
            return Ok(Employees);
        }
        
        // GET api/employee/1
        [HttpGet("{id}")]
        public ActionResult<Employee> GetById(int id)
        {
            var employee = Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }
        
        // POST api/employee
        [HttpPost]
        public ActionResult<Employee> Create(Employee newEmployee)
        {
            newEmployee.Id = Employees.Max(e => e.Id) + 1;
            Employees.Add(newEmployee);
            return CreatedAtAction(nameof(GetById), new { id = newEmployee.Id }, newEmployee);
        }
        
        // DELETE api/employee/1
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var employee = Employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
                return NotFound();
            Employees.Remove(employee);
            return NoContent();
        }
    }
}