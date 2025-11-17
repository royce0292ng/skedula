using System.ComponentModel.DataAnnotations;

namespace Skedula.Api.Models;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
        
    [Required, MaxLength(100)]
    public string Name { get; set; }
        
    [Required, MaxLength(20)]
    public string EmployeeCode { get; set; }
        
    public DateTime DateJoined { get; set; }
    public bool IsActive { get; set; } = true;
    public int MaxConsecutiveWorkDays { get; set; } = 6;
        
    public virtual ICollection<Schedule> Schedules { get; set; }
    public virtual EmployeeFatigueMetric FatigueMetric { get; set; }
}