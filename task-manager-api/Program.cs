using Microsoft.EntityFrameworkCore;
using RealTimeTaskManager.API.Data;
using TaskManager.API.DTOs;
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

// Simple get to return the tasks
app.MapGet("/api/tasks", async (TaskDbContext db) =>
{
    var tasks = await db.Tasks.OrderByDescending(x => x.CreateAt).ToListAsync();
    return tasks.Select(task => new TaskDto(task.Id, task.Title, task.TaskDescription, task.CreateAt));
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
