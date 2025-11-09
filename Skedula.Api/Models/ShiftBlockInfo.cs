namespace Skedula.Api.Models;

public class ShiftBlockInfo
{
    public ShiftBlock Code { get; set; }
    public string Description { get; set; } = "";
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    
    public TimeSpan Duration
    {
        get
        {
            // Handle overnight shifts if ever needed later (e.g. 22:00 → 06:00)
            if (End < Start)
                return (TimeSpan.FromHours(24) - Start) + End;

            return End - Start;
        }
    }
}