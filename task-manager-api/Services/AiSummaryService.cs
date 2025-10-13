using System.Text.Json;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services;

// Interface for using it as a Inject Service
public interface IAiSummaryService
{
    Task<string> GenerateSummaryAsync(IEnumerable<TaskDto> tasks);
}

public class AiSummaryService : IAiSummaryService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiSummaryService> _logger;

    public AiSummaryService(HttpClient httpClient, IConfiguration configuration, ILogger<AiSummaryService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateSummaryAsync(IEnumerable<TaskDto> tasks)
    {
        try
        {
            // Lists the tasks
            var tasksList = tasks.ToList();

            if (!tasksList.Any())
            {
                return "Not found tasks to summarize";
            }

            // Fallback for local summary api
            return GenerateLocalSummary(tasksList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary. Using fallback.");
            return GenerateLocalSummary(tasks.ToList());
        }
    }

    private string GenerateLocalSummary(List<TaskDto> tasks)
    {
        if (!tasks.Any())
            return "No tasks available.";

        var totalTasks = tasks.Count;
        var recentTasks = tasks.OrderByDescending(t => t.CreatedAt).Take(3);

        var summary = $"Summary for ({totalTasks} total)\n\n";

        summary += "Recent tasks:\n";
        foreach (var task in recentTasks)
        {
            summary += $"• {task.Title}: {task.Description}\n";
        }

        if (totalTasks > 3)
        {
            summary += $"\n... and more {totalTasks - 3} added tasks.";
        }

        return summary;
    } 
}