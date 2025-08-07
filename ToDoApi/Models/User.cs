namespace ToDoApi.Models;

public class User
{
    public Guid Id { get; init; }
    public string Email { get; init;} = string.Empty;
    public string PasswordHash { get; init;} = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init;}
}