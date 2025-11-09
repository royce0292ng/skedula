using Skedula.Api.Models;

namespace Skedula.Api.Services;

public static class ShiftBlockProvider
{
    // ✅ Dictionary for fast lookups
        public static readonly Dictionary<ShiftBlock, ShiftBlockInfo> All = new()
        {
            [ShiftBlock.D] = new ShiftBlockInfo
            {
                Code = ShiftBlock.D,
                Description = "Day Shift",
                Start = new TimeSpan(7, 45, 0),
                End = new TimeSpan(15, 20, 0),
            },
            [ShiftBlock.A] = new ShiftBlockInfo
            {
                Code = ShiftBlock.A,
                Description = "AM Shift",
                Start = new TimeSpan(7, 0, 0),
                End = new TimeSpan(14, 35, 0),
            },
            [ShiftBlock.E] = new ShiftBlockInfo
            {
                Code = ShiftBlock.E,
                Description = "Evening Shift",
                Start = new TimeSpan(13, 0, 0),
                End = new TimeSpan(20, 35, 0),
            },
            [ShiftBlock.EC] = new ShiftBlockInfo
            {
                Code = ShiftBlock.EC,
                Description = "Evening Cover",
                Start = new TimeSpan(13, 0, 0),
                End = new TimeSpan(20, 35, 0),
            },
            [ShiftBlock.HC] = new ShiftBlockInfo
            {
                Code = ShiftBlock.HC,
                Description = "Holiday On Call",
                Start = TimeSpan.Zero,
                End = TimeSpan.Zero,
            },
            [ShiftBlock.P] = new ShiftBlockInfo
            {
                Code = ShiftBlock.P,
                Description = "Special Duty",
                Start = TimeSpan.Zero,
                End = TimeSpan.Zero,
            },
            [ShiftBlock.CO] = new ShiftBlockInfo
            {
                Code = ShiftBlock.CO,
                Description = "Compensate Off (Every 2 Weeks)",
                Start = TimeSpan.Zero,
                End = TimeSpan.Zero,
            },
            [ShiftBlock.AL] = new ShiftBlockInfo
            {
                Code = ShiftBlock.AL,
                Description = "Annual Leave",
                Start = TimeSpan.Zero,
                End = TimeSpan.Zero,
            }
        };

        // Optional helper to get all items as a list
        public static List<ShiftBlockInfo> GetAll() => All.Values.ToList();
}