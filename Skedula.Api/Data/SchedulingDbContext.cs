using Microsoft.EntityFrameworkCore;

using Skedula.Api.Models;

namespace Skedula.Api.Data;

public class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<ShiftType> ShiftTypes { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<EmployeeFatigueMetric> EmployeeFatigueMetrics { get; set; }
    public DbSet<ShiftRotationTracker> ShiftRotationTrackers { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraint on Employee + Date
        modelBuilder.Entity<Schedule>()
            .HasIndex(s => new { s.EmployeeId, s.Date })
            .IsUnique();

        // Seed shift types
        modelBuilder.Entity<ShiftType>().HasData(
            new ShiftType { ShiftTypeId = 1, Code = "A", Name = "A Shift", RequiredHeadcount = 2, Priority = 1 },
            new ShiftType { ShiftTypeId = 2, Code = "C", Name = "C Shift", RequiredHeadcount = 3, Priority = 2 },
            new ShiftType { ShiftTypeId = 3, Code = "D", Name = "D Shift", RequiredHeadcount = 0, Priority = 3 },
            new ShiftType { ShiftTypeId = 4, Code = "E", Name = "E Shift", RequiredHeadcount = 3, Priority = 4 },
            new ShiftType { ShiftTypeId = 5, Code = "CO", Name = "Day Off", RequiredHeadcount = 0, IsRestDay = true, Priority = 5 },
            new ShiftType { ShiftTypeId = 6, Code = "EC", Name = "EC Shift", RequiredHeadcount = 1, Priority = 6 },
            new ShiftType { ShiftTypeId = 7, Code = "AL", Name = "Annual Leave", RequiredHeadcount = 0, IsRestDay = true, IsLeave = true, Priority = 7 }
        );
    }
}