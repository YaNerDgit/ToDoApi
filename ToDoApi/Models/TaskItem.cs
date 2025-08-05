namespace ToDoApi.Models;

public class TaskItem
{
    public int Id { get; init; }
    public string Title { get; init; } =  string.Empty;
    public string? Description { get; init; }
    public bool IsCompleted { get; init; } = false;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}