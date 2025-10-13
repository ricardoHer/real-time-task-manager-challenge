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
        
        // Try GitHub Models first, then OpenAI as fallback
        var githubKey = _configuration["GitHubModelsKey"];
        var openApiKey = _configuration["ChatGPTKey"];
        
        if (!string.IsNullOrEmpty(githubKey))
        {
            _logger.LogInformation("Using GitHub Models for AI summary");
            return await CallAiApiAsync(tasks, githubKey, "https://models.inference.ai.azure.com/chat/completions", "gpt-4o-mini");
        }
        else if (!string.IsNullOrEmpty(openApiKey))
        {
            _logger.LogInformation("Using OpenAI for AI summary");
            return await CallAiApiAsync(tasks, openApiKey, "https://api.openai.com/v1/chat/completions", "gpt-3.5-turbo");
        }
        else
        {
            _logger.LogWarning("No AI API keys found in configuration. Using local fallback.");
            return GenerateLocalSummary(tasks);
        }
    }

    private async Task<string> CallAiApiAsync(List<TaskDto> tasks, string apiKey, string endpoint, string model)
    {
        var tasksText = string.Join("\n", tasks.Select(t => $"- {t.Title}: {t.Description} "));

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

        var jsonContent = JsonSerializer.Serialize(requestBody);
        _logger.LogInformation("Sending request to OpenAI: {JsonContent}", jsonContent);
        
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            _logger.LogError("OpenAI API call failed. Status: {StatusCode}, Response: {Response}", 
                response.StatusCode, errorResponse);
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