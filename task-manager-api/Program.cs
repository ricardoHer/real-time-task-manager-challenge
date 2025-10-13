using Microsoft.EntityFrameworkCore;
using RealTimeTaskManager.API.Data;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
