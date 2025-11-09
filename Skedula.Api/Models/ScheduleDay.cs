namespace Skedula.Api.Models;

public class ScheduleDay
{
    public DateTime Date { get; set; }
    public List<Shift> Shifts { get; set; } = new();
}