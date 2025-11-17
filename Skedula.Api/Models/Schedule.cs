using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skedula.Api.Models;

public class Schedule
{
    [Key]
    public int ScheduleId { get; set; }
        
    [Required]
    public int EmployeeId { get; set; }
        
    [Required]
    public DateTime Date { get; set; }
        
    [Required]
    public int ShiftTypeId { get; set; }
        
    public bool IsAutoAssigned { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; }
        
    [ForeignKey("ShiftTypeId")]
    public virtual ShiftType ShiftType { get; set; }
}
