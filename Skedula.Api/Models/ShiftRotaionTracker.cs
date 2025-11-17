using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skedula.Api.Models;

public class ShiftRotationTracker
{
    [Key]
    public int TrackerId { get; set; }
        
    [Required]
    public int ShiftTypeId { get; set; }
        
    [Required, MaxLength(50)]
    public string RotationType { get; set; }
        
    public int? LastAssignedEmployeeId { get; set; }
    public DateTime? LastAssignedDate { get; set; }
    public string RotationSequence { get; set; } // JSON array
    public int CurrentIndex { get; set; }
        
    [ForeignKey("ShiftTypeId")]
    public virtual ShiftType ShiftType { get; set; }
}