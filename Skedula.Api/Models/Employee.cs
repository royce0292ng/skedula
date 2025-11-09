namespace Skedula.Api.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Role { get; set; } = null!;
    public required string Group { get; set; }
}