namespace ToDoApi.Contracts;

public record RegisterRequest(string Email, string Password, string FullName);