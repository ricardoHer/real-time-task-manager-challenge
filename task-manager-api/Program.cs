using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealTimeTaskManager.API.Data;
using RealTimeTaskManager.API.Data.Models;
using TaskManager.API.DTOs;
using TaskManager.API.Hubs;
using TaskManager.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add local settings configuration (if file exists)
if (File.Exists("localsettings.json"))
{
    builder.Configuration.AddJsonFile("localsettings.json", optional: true, reloadOnChange: true);
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddDbContext<TaskDbContext>(options => options.UseInMemoryDatabase("SqliteConnection"));

// Adding the services, for HTTPClient consumption and for Service
builder.Services.AddHttpClient<IAiSummaryService, AiSummaryService>();
builder.Services.AddScoped<IAiSummaryService, AiSummaryService>();

// Configuring Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:5261", "https://localhost:7000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply CORS policy
app.UseCors("AllowReactApp");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Scaffolded api action
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// API Application Endpoints

// Map the signalR hub
app.MapHub<TaskHub>("/taskhub");

// Simple get to return the tasks
app.MapGet("/api/tasks", async (TaskDbContext db) =>
{
    var tasks = await db.Tasks.OrderByDescending(x => x.CreateAt).ToListAsync();
    return tasks.Select(task => new TaskDto(task.Id, task.Title, task.TaskDescription, task.CreateAt));
});

app.MapPost("/api/tasks", async (CreateTaskDto createTaskDto, TaskDbContext db, IHubContext<TaskHub> hubContext) =>
{
    if (string.IsNullOrEmpty(createTaskDto.Title) || string.IsNullOrEmpty(createTaskDto.Description))
    {
        return Results.BadRequest("Title and description are required");
    }

    var task = new TaskItem
    {
        Title = createTaskDto.Title.Trim(),
        TaskDescription = createTaskDto.Description.Trim(),
        CreateAt = DateTime.Now
    };

    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    var taskDto = new TaskDto(task.Id, task.Title, task.TaskDescription, task.CreateAt);

    // Notifying all connected clients
    await hubContext.Clients.All.SendAsync("TaskAdded", taskDto);

    return Results.Created($"/api/tasks/{task.Id}", taskDto);
});

app.MapPost("/api/tasks/summary", async (TaskDbContext db, IAiSummaryService aiService) =>
{
    var tasks = await db.Tasks.OrderByDescending(x => x.CreateAt).ToListAsync();
    var tasksDto = tasks.Select(x => new TaskDto(x.Id, x.Title, x.TaskDescription, x.CreateAt));

    var summary = await aiService.GenerateSummaryAsync(tasksDto);

    return Results.Ok(new TaskSummaryDto(summary, tasks.Count));
});

// Health check endpoint
app.MapGet("/", () => "Task Manager API is running!");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
