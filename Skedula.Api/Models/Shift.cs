namespace Skedula.Api.Models;

public class Shift
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public required ShiftBlockInfo ShiftBlock { get; set; }
}