using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Contracts;
using ToDoApi.Data;
using ToDoApi.Models;

namespace ToDoApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController(AppDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userGuid = Guid.Parse(userId);
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            UserId = userGuid,
            User = context.Users.FirstOrDefault(u => u.Id == userGuid)
        };
        await context.Tasks.AddAsync(task, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userGuid = Guid.Parse(userId);
        var taskDtos = await context.Tasks
            .Where(t => t.UserId == userGuid)
            .Select(t => new TaskDto(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt))
            .ToListAsync(cancellationToken);
        return Ok(new GetTasksResponse(taskDtos));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userGuid = Guid.Parse(userId);
        var taskQuery = context.Tasks
            .Where(t => t.Id == id && t.UserId == userGuid);
        if (!taskQuery.Any())
        {
            return NotFound();
        }

        var taskDtos = await taskQuery
            .Select(t => new TaskDto(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(new GetTasksResponse(taskDtos));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userGuid = Guid.Parse(userId);
        var task = await context.Tasks.FindAsync(id, cancellationToken);
        if (task == null)
        {
            return NotFound();
        }

        if (task.UserId != userGuid)
        {
            return Forbid();
        }

        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTask([FromRoute] int id, PutTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userGuid = Guid.Parse(userId);
        
        var task = context.Tasks.FirstOrDefault(t => t.Id == id && t.UserId == userGuid);
        if (task == null)
        {
            return NotFound();
        }
        
        var updatedTask = new TaskItem
        {
            Id = task.Id,
            Title = request.Title,
            Description = request.Description,
            UserId = task.UserId,
            IsCompleted = request.IsCompleted,
            User = task.User
        };
        context.Tasks.Attach(updatedTask);
        
        context.Entry(updatedTask).Property(t => t.Title).IsModified = true;
        context.Entry(updatedTask).Property(t => t.Description).IsModified = true;
        context.Entry(updatedTask).Property(t => t.IsCompleted).IsModified = true;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/tasks")]
    public async Task<IActionResult> GetAllTasks(CancellationToken cancellationToken)
    {
        var taskDtos = await context.Tasks
            .Select(t => new TaskDto(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt))
            .ToListAsync(cancellationToken);
        return Ok(new GetTasksResponse(taskDtos));
    }
}