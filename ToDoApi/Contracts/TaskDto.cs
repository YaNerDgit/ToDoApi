namespace ToDoApi.Contracts;

public record TaskDto(int Id, string Title, string Description, bool IsCompleted, DateTime CreatedAt);