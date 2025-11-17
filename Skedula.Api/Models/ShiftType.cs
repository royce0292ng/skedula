using System.ComponentModel.DataAnnotations;

namespace Skedula.Api.Models;

public class ShiftType
{
    [Key]
    public int ShiftTypeId { get; set; }
        
    [Required, MaxLength(10)]
    public string Code { get; set; }
        
    [Required, MaxLength(50)]
    public string Name { get; set; }
        
    public int RequiredHeadcount { get; set; }
    public bool IsRestDay { get; set; }
    public bool IsLeave { get; set; }
    public int Priority { get; set; }
        
    public virtual ICollection<Schedule> Schedules { get; set; }
}