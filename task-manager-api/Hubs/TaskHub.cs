using Microsoft.AspNetCore.SignalR;

namespace TaskManager.API.Hubs;

public class TaskHub : Hub
{
    public async Task SendTaskUpdate(object taskData)
    {
        await Clients.All.SendAsync("TaskAdded", taskData);
    }

    public override async Task OnConnectedAsync()
    {
        // May implement a kind of cache to return lost message if is needed
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}