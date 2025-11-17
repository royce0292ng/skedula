using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skedula.Api.Models;

public class LeaveRequest
{
    [Key]
    public int LeaveId { get; set; }
        
    [Required]
    public int EmployeeId { get; set; }
        
    [Required]
    public DateTime StartDate { get; set; }
        
    [Required]
    public DateTime EndDate { get; set; }
        
    [Required, MaxLength(50)]
    public string LeaveType { get; set; }
        
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";
        
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; }
}