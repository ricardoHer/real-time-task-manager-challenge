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

            return await GenerateGitHubModelsSummaryAsync(tasksList);

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

    private async Task<string> GenerateGitHubModelsSummaryAsync(List<TaskDto> tasks)
    {
        var githubKey = _configuration["GitHubModelsKey"];
        
        if (string.IsNullOrEmpty(githubKey))
        {
            _logger.LogWarning("GitHub Models API key not found in configuration. Using local fallback.");
            return GenerateLocalSummary(tasks);
        }

        _logger.LogInformation("Using GitHub Models for AI summary");
        return await CallGitHubModelsApiAsync(tasks, githubKey);
    }

    private async Task<string> CallGitHubModelsApiAsync(List<TaskDto> tasks, string apiKey)
    {
        const string endpoint = "https://models.inference.ai.azure.com/chat/completions";
        const string model = "gpt-4o-mini";
        
        var tasksText = string.Join("\n", tasks.Select(t => $"- {t.Title}: {t.Description} "));

        var prompt = $"Summarize the following tasks in a concise and organized manner:\n\n{tasksText}\n\nProvide a summary in English highlighting the main topics and activities.";

        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = "You are an assistant who summarizes to-do lists in a clear and organized way." },
                new { role = "user", content = prompt }
            },
            max_tokens = 200,
            temperature = 0.7d
        };

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var jsonContent = JsonSerializer.Serialize(requestBody);
        _logger.LogInformation("Sending request to GitHub Models with model {Model}", model);
        
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            
            // Check for specific error types
            if (errorResponse.Contains("insufficient_quota"))
            {
                _logger.LogWarning("GitHub Models quota exceeded. Using local fallback summary.");
            }
            else if (errorResponse.Contains("invalid_api_key") || errorResponse.Contains("unauthorized"))
            {
                _logger.LogWarning("Invalid GitHub Models API key. Using local fallback summary.");
            }
            else
            {
                _logger.LogError("GitHub Models API call failed. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorResponse);
            }
            
            return GenerateLocalSummary(tasks);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("GitHub Models API Response: {Response}", responseContent);
        
        try 
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            
            // Try to get the content from choices[0].message.content
            if (root.TryGetProperty("choices", out var choices) && 
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var contentProperty))
                {
                    var summaryContent = contentProperty.GetString();
                    
                    if (!string.IsNullOrEmpty(summaryContent))
                    {
                        _logger.LogInformation("GitHub Models summary generated successfully");
                        return summaryContent;
                    }
                }
            }
            
            _logger.LogWarning("GitHub Models API returned empty content, using local fallback");
            return GenerateLocalSummary(tasks);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse GitHub Models API response. Response: {Response}", responseContent);
            return GenerateLocalSummary(tasks);
        }
    }
}

// Response models for GitHub Models API (Azure OpenAI format)
public record AiApiResponse(AiChoice[] Choices);

public record AiChoice(
    AiMessage Message, 
    string? Finish_Reason, 
    int Index,
    object? Logprobs = null,
    object? Content_Filter_Results = null
);

public record AiMessage(
    string Content, 
    string? Role = null, 
    string? Refusal = null,
    object[]? Annotations = null
);