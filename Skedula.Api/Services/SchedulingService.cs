using Skedula.Api.Models;

namespace Skedula.Api.Services;

public class SchedulingService
{
    private readonly Random _random = new();

    /// <summary>
    /// Generates a monthly roster, cycling shifts within each employee group.
    /// </summary>
    public List<ScheduleDay> GenerateMonthlySchedule(List<Employee> employees, int year, int month)
    {
        var schedule = new List<ScheduleDay>();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        int shiftId = 1;

        // Get only active shifts (exclude leave/off)
        var activeBlocks = ShiftBlockProvider.All.Values
            .Where(b => b.Code is not (ShiftBlock.HC or ShiftBlock.P or ShiftBlock.CO or ShiftBlock.AL))
            .ToList();

        // Group employees (e.g. Group A, Group B)
        var groups = employees.GroupBy(e => e.Group).ToList();

        // Assign each group a different starting offset
        var groupOffsets = groups.Select((g, index) => new
        {
            GroupName = g.Key,
            Offset = index % activeBlocks.Count // e.g. 0,1,2, then repeat
        }).ToDictionary(x => x.GroupName, x => x.Offset);

        // Loop through all days of the month
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            var scheduleDay = new ScheduleDay { Date = date };

            foreach (var group in groups)
            {
                var offset = groupOffsets[group.Key];

                foreach (var emp in group)
                {
                    // Cycle within the group: use day + offset + emp.Id for variation
                    var index = (day + offset + emp.Id) % activeBlocks.Count;
                    var selectedBlock = activeBlocks[index];

                    scheduleDay.Shifts.Add(new Shift
                    {
                        Id = shiftId++,
                        EmployeeId = emp.Id,
                        ShiftBlock = selectedBlock,
                        Date = selectedBlock.Date
                        // StartTime = date + selectedBlock.Start,
                        // EndTime = date + selectedBlock.End,
                        // BlockCode = selectedBlock.Code.ToString(),
                        // BlockDescription = selectedBlock.Description
                    });
                }
            }

            schedule.Add(scheduleDay);
        }

        return schedule;
    }
}
