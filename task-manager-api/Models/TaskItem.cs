namespace RealTimeTaskManager.API.Data.Models;

public class TaskItem
{
    public int Id { get; set; }
    public required string TaskDescription { get; set; }
    public required string Title { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.Now;
}