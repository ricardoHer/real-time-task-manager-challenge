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
        _logger.LogInformation("Sending request to {Endpoint} with model {Model}", endpoint, model);
        
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            
            // Check for specific error types
            if (errorResponse.Contains("insufficient_quota"))
            {
                _logger.LogWarning("AI API quota exceeded. Using local fallback summary.");
            }
            else if (errorResponse.Contains("invalid_api_key") || errorResponse.Contains("unauthorized"))
            {
                _logger.LogWarning("Invalid AI API key. Using local fallback summary.");
            }
            else
            {
                _logger.LogError("AI API call failed. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorResponse);
            }
            
            return GenerateLocalSummary(tasks);
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("AI API Response: {Response}", responseContent);
        
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
                        _logger.LogInformation("AI summary generated successfully");
                        return summaryContent;
                    }
                }
            }
            
            _logger.LogWarning("AI API returned empty content, using local fallback");
            return GenerateLocalSummary(tasks);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse AI API response. Response: {Response}", responseContent);
            return GenerateLocalSummary(tasks);
        }
    }
}

// Updated response models to handle both OpenAI and GitHub Models (Azure OpenAI) formats
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