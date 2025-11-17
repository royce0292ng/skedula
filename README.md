# skedula
Schedula for roostering

## Shifts

39hr Per Week 
0.5 hr Lunch Per Shift Include.

| Shift Block  | Description | Hours |
| ---- | -------------- |------|
| D    | 07:45 to 15:20 | 7:35 |
| A    | 07:00 to 14:35 | 7:35 |
| E    | 13:00 to 20:35 | 7:35 |
| EC   | 13:00 to 20:35 | 7:35 |
| HC   | Holiday On call| 0:00 |
| P    | Special Duty   | 0:00 |
| CO   | Compensate off : Every 2 Weeks | 0:00 |
| AL   | Annual leave   | 0:00 |


## USAGE INSTRUCTIONS

1. Create new .NET Web API project:
```
   dotnet new webapi -n EmployeeScheduling
```

2. Install required packages:
```
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   dotnet add package Swashbuckle.AspNetCore
```
3. Copy all code above into appropriate files:
   - Models/ folder for all model classes
   - Data/ folder for SchedulingDbContext
   - Services/ folder for all service classes
   - Controllers/ folder for all controllers
   - Program.cs for startup configuration

4. Update appsettings.json with your database connection string

5. Create and apply migrations:
```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
```
6. Run the application:
```
   dotnet run
```
7. Access Swagger UI at: ```https://localhost:5001/swagger```

8. API Endpoints:

   Scheduling:
   - POST /api/schedule/generate - Generate schedule for date range
   - GET /api/schedule/range?startDate={date}&endDate={date} - Get schedules
   - GET /api/schedule/employee/{id} - Get employee schedule
   - PUT /api/schedule/{id} - Update schedule
   - DELETE /api/schedule/{id} - Delete schedule
   - GET /api/schedule/validate?date={date} - Validate schedule

   Employees:
   - GET /api/employee - Get all employees
   - GET /api/employee/{id} - Get employee by ID
   - POST /api/employee - Create employee
   - PUT /api/employee/{id} - Update employee
   - GET /api/employee/{id}/fatigue - Get fatigue metrics

   Leave:
   - GET /api/leave - Get all leave requests
   - GET /api/leave/employee/{id} - Get employee leaves
   - POST /api/leave - Create leave request
   - PUT /api/leave/{id}/approve - Approve leave
   - PUT /api/leave/{id}/reject - Reject leave
   - DELETE /api/leave/{id} - Delete leave

9. Example API calls:

   Generate Schedule:
```
   POST /api/schedule/generate
   {
     "startDate": "2025-11-17",
     "endDate": "2025-11-30"
   }
```
   Create Employee:
```
   POST /api/employee
   {
     "name": "John Doe",
     "employeeCode": "E001",
     "dateJoined": "2025-01-01",
     "maxConsecutiveWorkDays": 6
   }
```
   Create Leave Request:
```
   POST /api/leave
   {
     "employeeId": 1,
     "startDate": "2025-11-20",
     "endDate": "2025-11-22",
     "leaveType": "Annual Leave"
   }
```
10. Database will automatically:
    - Track fatigue metrics
    - Enforce hard constraints (headcounts)
    - Apply rotation rules
    - Handle weekend C → Monday E rule
    - Assign CO every 14 days
    - Score and optimize assignments
*/