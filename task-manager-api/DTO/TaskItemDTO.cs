namespace TaskManager.API.DTOs;

public record CreateTaskDto(string Title, string Description);

public record TaskDto(int Id, string Title, string Description, DateTime CreatedAt);

public record TaskSummaryDto(string Summary, int TotalTasks);