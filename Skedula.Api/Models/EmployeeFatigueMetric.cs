using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skedula.Api.Models;

public class EmployeeFatigueMetric
{
    [Key]
    public int MetricId { get; set; }
        
    [Required]
    public int EmployeeId { get; set; }
        
    public DateTime WeekStartDate { get; set; }
    public int ConsecutiveWorkDays { get; set; }
    public int TotalWorkDaysInPeriod { get; set; }
    public int WeekendShiftsCount { get; set; }
    public DateTime? LastCODate { get; set; }
    public DateTime? LastEDate { get; set; }
    public DateTime? LastCOnWeekendDate { get; set; }
    public double FatigueScore { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; }
}