# Skedula - Employee Scheduling System

An intelligent employee scheduling system built with .NET 10 and PostgreSQL that automatically generates optimized work schedules while managing fatigue, rotations, and leave requests.

## Table of Contents

- [Features](#features)
- [Shift Types](#shift-types)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [System Architecture](#system-architecture)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)

---

## Features

✨ **Intelligent Scheduling**
- Automatic schedule generation with constraint satisfaction
- Fatigue tracking and management
- Fair rotation distribution
- Weekend work balancing

🔄 **Business Rules**
- CO (Compensate Off) assignment every 2 weeks
- Weekend C shift → Monday E shift rule
- Configurable maximum consecutive work days
- Shift headcount enforcement (A=2, C=3, E=3)

📊 **Employee Management**
- Employee CRUD operations
- Fatigue score calculation
- Work history tracking
- Active/inactive status management

🏖️ **Leave Management**
- Leave request workflow
- Approval/rejection system
- Integration with scheduling algorithm

---

## Shift Types

**Working Hours:** 39 hours per week (7.35 hours per shift, 0.5hr lunch included)

| Shift Code | Description | Hours | Required Headcount | Notes |
|------------|-------------|-------|-------------------|-------|
| **D** | Day Shift | 07:45 - 15:20 (7:35) | Dynamic | Default shift |
| **A** | A Shift | 07:00 - 14:35 (7:35) | 2 | Morning shift |
| **E** | E Shift | 13:00 - 20:35 (7:35) | 3 | Evening shift |
| **C** | C Shift | (Varies) | 3 | Variable shift |
| **EC** | EC Shift | 13:00 - 20:35 (7:35) | 1 | Evening coverage |
| **CO** | Compensate Off | N/A | N/A | Day off (every 2 weeks) |
| **AL** | Annual Leave | N/A | N/A | Approved leave |
| **HC** | Holiday On-call | N/A | 0 | On-call duty |
| **P** | Special Duty | N/A | 0 | Special assignments |

---

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET SDK 10.0 or higher** - [Download here](https://dotnet.microsoft.com/download)
- **Docker** - [Install Docker](https://docs.docker.com/get-docker/)
- **Git** (optional) - For cloning the repository

### Verify Installation

```bash
# Check .NET version
dotnet --version
# Should output: 10.0.x or higher

# Check Docker
docker --version
# Should output: Docker version 20.x or higher
```

---

## Installation

### 1. Clone or Download the Repository

```bash
git clone https://github.com/royce0292ng/skedula.git
cd skedula/Skedula.Api
```

Or download and extract the ZIP file.

### 2. Install Required Packages

```bash
# Navigate to the API project
cd Skedula.Api

# Restore NuGet packages
dotnet restore

# Install required packages
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 10.0.0
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

### 3. Install EF Core Tools

```bash
# Install Entity Framework Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

---

## Database Setup

### 1. Start PostgreSQL Database

```bash
# Run PostgreSQL in Docker
docker run -d \
  --name postgres-skedula \
  -e POSTGRES_DB=skeduladb \
  -e POSTGRES_USER=dbadmin \
  -e POSTGRES_PASSWORD=skedula \
  -p 5432:5432 \
  --restart unless-stopped \
  postgres:16

# Verify container is running
docker ps | grep postgres-skedula
```

### 2. Configure Connection String

The connection string is in `appsettings.json` (already configured):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=skeduladb;Username=dbadmin;Password=skedula;Port=5432"
  }
}
```

### 3. Create and Apply Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### 4. Verify Database Setup

```bash
# Connect to the database
docker exec -it postgres-skedula psql -U dbadmin -d skeduladb

# List all tables
\dt

# Expected output:
#  - Employees
#  - EmployeeFatigueMetrics
#  - LeaveRequests
#  - Schedules
#  - ShiftRotationTrackers
#  - ShiftTypes
#  - __EFMigrationsHistory

# Check seed data
SELECT * FROM "ShiftTypes";

# Exit
\q
```

---

## Running the Application

### Development Mode

```bash
# Run with hot reload (recommended for development)
dotnet watch

# Or standard run
dotnet run
```

### Access the Application

Once running, the application will be available at:

- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger UI:** http://localhost:5000/swagger

### Swagger UI

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

This provides an interactive API documentation where you can:
- View all available endpoints
- Test API calls directly from the browser
- See request/response schemas

---

## API Documentation

### Base URL
```
http://localhost:5000/api
```

### Employee Endpoints

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| GET | `/employee` | Get all active employees | - |
| GET | `/employee?includeInactive=true` | Get all employees including inactive | - |
| GET | `/employee/{id}` | Get employee by ID | - |
| POST | `/employee` | Create new employee | [CreateEmployeeRequest](#createemployeerequest) |
| PUT | `/employee/{id}` | Update employee | [UpdateEmployeeRequest](#updateemployeerequest) |
| DELETE | `/employee/{id}` | Delete employee (soft delete if has schedules) | - |
| PUT | `/employee/{id}/reactivate` | Reactivate deactivated employee | - |
| GET | `/employee/{id}/fatigue` | Get employee fatigue metrics | - |

#### CreateEmployeeRequest
```json
{
  "name": "John Doe",
  "employeeCode": "E001",
  "dateJoined": "2025-01-01",
  "maxConsecutiveWorkDays": 6
}
```

#### UpdateEmployeeRequest
```json
{
  "name": "John Smith",
  "employeeCode": "E001",
  "maxConsecutiveWorkDays": 5,
  "isActive": true
}
```

### Schedule Endpoints

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| POST | `/schedule/generate` | Generate schedule for date range | [GenerateScheduleRequest](#generateschedulerequest) |
| GET | `/schedule/range` | Get schedules for date range | Query: `startDate`, `endDate` |
| GET | `/schedule/employee/{id}` | Get employee's schedule | Query: `startDate`, `endDate` |
| PUT | `/schedule/{id}` | Update schedule | [UpdateScheduleRequest](#updateschedulerequest) |
| DELETE | `/schedule/{id}` | Delete schedule | - |
| GET | `/schedule/validate` | Validate schedule for date | Query: `date` |

#### GenerateScheduleRequest
```json
{
  "startDate": "2025-11-25",
  "endDate": "2025-12-08"
}
```

#### UpdateScheduleRequest
```json
{
  "shiftTypeId": 4,
  "notes": "Manual override"
}
```

### Leave Endpoints

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| GET | `/leave` | Get all leave requests | Query: `status` (optional) |
| GET | `/leave/employee/{id}` | Get employee's leave requests | - |
| POST | `/leave` | Create leave request | [CreateLeaveRequest](#createleaverequest) |
| PUT | `/leave/{id}/approve` | Approve leave request | - |
| PUT | `/leave/{id}/reject` | Reject leave request | - |
| DELETE | `/leave/{id}` | Delete leave request | - |

#### CreateLeaveRequest
```json
{
  "employeeId": 1,
  "startDate": "2025-12-01",
  "endDate": "2025-12-05",
  "leaveType": "Annual Leave"
}
```

---

## System Architecture

### Project Structure

```
Skedula.Api/
├── Controllers/
│   ├── EmployeeController.cs      # Employee CRUD operations
│   ├── ScheduleController.cs      # Schedule management
│   └── LeaveController.cs         # Leave request handling
├── Services/
│   ├── FatigueService.cs          # Fatigue calculation and tracking
│   ├── ConstraintService.cs       # Business rule validation
│   ├── RotationService.cs         # Shift rotation management
│   └── SchedulingService.cs       # Main scheduling algorithm
├── Models/
│   ├── Employee.cs                # Employee entity
│   ├── Schedule.cs                # Schedule entity
│   ├── ShiftType.cs               # Shift type definition
│   ├── EmployeeFatigueMetric.cs   # Fatigue tracking data
│   ├── ShiftRotationTracker.cs    # Rotation state
│   └── LeaveRequest.cs            # Leave request entity
├── Data/
│   └── SchedulingDbContext.cs     # EF Core database context
├── Program.cs                      # Application entry point
└── appsettings.json               # Configuration

```

### Key Components

#### 1. **SchedulingService** - Core Scheduling Engine

The main algorithm that generates optimized schedules.

**Key Methods:**
- `GenerateScheduleAsync(startDate, endDate)` - Generates complete schedule for date range
- `ProcessDayAsync(date, schedules)` - Processes scheduling for a single day
- `ValidateScheduleAsync(date)` - Validates schedule against constraints

**Scheduling Process:**
1. Update fatigue metrics for all employees
2. Assign approved leaves (highest priority)
3. Assign CO shifts (every 14 days)
4. Apply weekend C → Monday E rule
5. Process rotation assignments (Monday CO, Saturday EC, Saturday E)
6. Assign A, C, E shifts using scoring algorithm
7. Assign remaining employees to D shift

#### 2. **FatigueService** - Employee Workload Management

Tracks and calculates employee fatigue to prevent overwork.

**Key Methods:**
- `UpdateFatigueMetricsAsync(employeeId)` - Recalculates fatigue metrics
- `GetFatigueScoreAsync(employeeId)` - Returns current fatigue score
- `GetEligibleEmployeeIdsAsync(date, shiftCode)` - Returns available employees

**Fatigue Score Formula:**
```csharp
FatigueScore = 
  (ConsecutiveWorkDays × 2.0) +
  (TotalWorkDaysIn14Days / 14 × 10) +
  (WeekendShiftsLast4Weeks × 1.5) +
  (DaysSinceLastCO < 7 ? 0 : (DaysSinceLastCO - 7) × 0.5)
```

**Lower score = Less fatigued = Higher priority for work assignment**

#### 3. **ConstraintService** - Business Rule Enforcement

Validates and scores assignments based on business rules.

**Key Methods:**
- `ValidateHardConstraintsAsync(schedules)` - Ensures required headcounts
- `CalculateAssignmentScoreAsync(employee, date, shift)` - Scores assignment quality

**Hard Constraints:**
- A shift: exactly 2 people
- C shift: exactly 3 people
- E shift: exactly 3 people
- No double assignments
- CO every 14 days

**Soft Constraints (Scoring):**
```csharp
Score = 100 (base)
  - (FatigueScore × 10)
  - (ConsecutiveWorkDays × 5)
  - (WeekendShiftsCount × 3)
  + (DaysSinceLastSameShift × 0.5)
  + (RotationDue × 15)
  + (WeekendC→MondayE match × 30)
```

#### 4. **RotationService** - Fair Distribution

Manages rotation sequences for specific shift assignments.

**Tracked Rotations:**
- Monday CO assignments
- Saturday EC assignments
- Saturday E assignments

**Key Methods:**
- `GetNextInRotationAsync(rotationType, shiftTypeId)` - Gets next employee in rotation
- `UpdateRotationAsync(rotationType, shiftTypeId, employeeId, date)` - Updates rotation state
- `InitializeRotationsAsync()` - Sets up rotation sequences

---

## Configuration

### Application Settings

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=skeduladb;Username=dbadmin;Password=skedula;Port=5432"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### Employee Configuration

Each employee has:
- **MaxConsecutiveWorkDays** (default: 6) - Maximum days they can work consecutively
- **IsActive** - Whether the employee is currently active
- **DateJoined** - Used for calculating seniority

### Scheduling Rules

Configured in `ConstraintService` and `SchedulingService`:

```csharp
// Shift headcount requirements
A_SHIFT_COUNT = 2
C_SHIFT_COUNT = 3
E_SHIFT_COUNT = 3
D_SHIFT_COUNT = Remaining employees

// CO frequency
CO_CYCLE_DAYS = 14

// Fatigue limits
MAX_CONSECUTIVE_DAYS = 6 (configurable per employee)
FATIGUE_LOOKBACK_DAYS = 14
WEEKEND_SHIFTS_LOOKBACK_DAYS = 28
```

---

## Usage Examples

### 1. Set Up Initial Employees

```bash
# Create employees
curl -X POST http://localhost:5000/api/employee \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Johnson",
    "employeeCode": "E001",
    "dateJoined": "2024-01-01",
    "maxConsecutiveWorkDays": 6
  }'

curl -X POST http://localhost:5000/api/employee \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Bob Smith",
    "employeeCode": "E002",
    "dateJoined": "2024-01-01",
    "maxConsecutiveWorkDays": 6
  }'

# Continue creating until you have at least 8 employees
# (Required: 2 for A + 3 for C + 3 for E = minimum 8)
```

### 2. Generate Schedule

```bash
# Generate a 2-week schedule
curl -X POST http://localhost:5000/api/employee \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2025-12-01",
    "endDate": "2025-12-14"
  }'
```

### 3. Request Leave

```bash
# Create leave request
curl -X POST http://localhost:5000/api/leave \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": 1,
    "startDate": "2025-12-05",
    "endDate": "2025-12-07",
    "leaveType": "Annual Leave"
  }'

# Approve leave
curl -X PUT http://localhost:5000/api/leave/1/approve
```

### 4. View and Modify Schedules

```bash
# Get schedule for date range
curl "http://localhost:5000/api/schedule/range?startDate=2025-12-01&endDate=2025-12-14"

# Get specific employee's schedule
curl "http://localhost:5000/api/schedule/employee/1?startDate=2025-12-01&endDate=2025-12-14"

# Update a schedule (manual override)
curl -X PUT http://localhost:5000/api/schedule/1 \
  -H "Content-Type: application/json" \
  -d '{
    "shiftTypeId": 5,
    "notes": "Manual CO assignment"
  }'
```

### 5. Check Employee Fatigue

```bash
# Get fatigue metrics for employee
curl http://localhost:5000/api/employee/1/fatigue
```

---

## Database Management

### Backup Database

```bash
# Create backup
docker exec postgres-skedula pg_dump -U dbadmin skeduladb > backup_$(date +%Y%m%d).sql

# Restore from backup
docker exec -i postgres-skedula psql -U dbadmin skeduladb < backup_20251124.sql
```

### View Database Contents

```bash
# Connect to database
docker exec -it postgres-skedula psql -U dbadmin -d skeduladb

# Useful queries
SELECT * FROM "Employees" WHERE "IsActive" = true;
SELECT * FROM "Schedules" WHERE "Date" >= CURRENT_DATE ORDER BY "Date";
SELECT e."Name", COUNT(s.*) as shifts FROM "Employees" e 
  LEFT JOIN "Schedules" s ON e."EmployeeId" = s."EmployeeId" 
  GROUP BY e."Name";

# Exit
\q
```

### Reset Database

```bash
# Drop and recreate
dotnet ef database drop --force
dotnet ef database update
```

---

## Troubleshooting

### Common Issues

#### 1. Port 5432 Already in Use

```bash
# Check what's using the port
sudo ss -tulpn | grep 5432

# Stop native PostgreSQL
sudo systemctl stop postgresql

# Or use different port
docker run -d --name postgres-skedula -p 5433:5432 ...
# Then update connection string to Port=5433
```

#### 2. Migration Errors

```bash
# Clean and recreate migrations
rm -rf Migrations/
dotnet clean
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 3. Circular Reference JSON Error

Ensure `Program.cs` has:
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
```

#### 4. DateTime/TimeZone Errors

Ensure `Program.cs` has before `AddDbContext`:
```csharp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
```

#### 5. Not Enough Employees for Schedule

Error: Cannot generate schedule

Solution: Ensure you have at least:
- 2 employees for A shift
- 3 employees for C shift
- 3 employees for E shift
- Minimum 8 active employees total

### Docker Issues

```bash
# View container logs
docker logs postgres-skedula

# Restart container
docker restart postgres-skedula

# Remove and recreate
docker rm -f postgres-skedula
# Then run docker run command again
```

### Application Logs

Enable detailed logging in `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## Support

For issues, questions, or contributions:
- **Issues:** [GitHub Issues](https://github.com/yourusername/skedula/issues)
- **Discussions:** [GitHub Discussions](https://github.com/yourusername/skedula/discussions)

---

## Acknowledgments

Built with:
- .NET 10
- Entity Framework Core
- PostgreSQL
- Swagger/OpenAPI
- Docker

---

**Happy Scheduling! 📅**