using Microsoft.EntityFrameworkCore;
using Skedula.Api.Data;
using Skedula.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<SchedulingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IFatigueService, FatigueService>();
builder.Services.AddScoped<IConstraintService, ConstraintService>();
builder.Services.AddScoped<IRotationService, RotationService>();
builder.Services.AddScoped<ISchedulingService, SchedulingService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Initialize database and rotations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
    var rotationService = scope.ServiceProvider.GetRequiredService<IRotationService>();
    
    // Apply migrations
    context.Database.Migrate();
    
    // Initialize rotations
    await rotationService.InitializeRotationsAsync();
}

app.Run();