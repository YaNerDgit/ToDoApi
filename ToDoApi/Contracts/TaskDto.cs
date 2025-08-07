namespace ToDoApi.Contracts;

public record TaskDto(int Id, string Name, string Description, bool IsCompleted, DateTime CreatedAt);