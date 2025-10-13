using System.Text.Json;
using Microsoft.OpenApi.Models;
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

            return await GeneratingOpenAISummaryAsync(tasksList);

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

    private async Task<string> GeneratingOpenAISummaryAsync(List<TaskDto> tasks)
    {
        var tasksText = string.Join("\n", tasks.Select(t => $"- {t.Title}: {t.Description} "));
        // getting the api key from the configuration file
        var openApiKey = _configuration["ChatGPTKey"];

        var prompt = $"Summarize the following tasks in a concise and organized manner:\n\n{tasksText}\n\nProvide a summary in Brazilian Portuguese highlighting the main topics and activities.";

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "You are an assistant who summarizes to-do lists in a clear and organized way." },
                new { role = "user", content = prompt }
            },
            max_tokens = 200,
            temperature = 0.7d
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {openApiKey}");

        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Open API call failed. using local fallback.");
            return GenerateLocalSummary(tasks);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);

        return openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? GenerateLocalSummary(tasks);
    }
}

public record OpenAiResponse(OpenAiChoice[] Choices);
public record OpenAiChoice(OpenAiMessage Message);
public record OpenAiMessage(string Content);