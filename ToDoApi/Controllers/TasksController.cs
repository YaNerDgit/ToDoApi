using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Contracts;
using ToDoApi.Data;
using ToDoApi.Models;

namespace ToDoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(AppDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
        };
        await context.Tasks.AddAsync(task, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(CancellationToken cancellationToken)
    {
        var taskDtos = await context.Tasks
            .Select(t => new TaskDto(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt))
            .ToListAsync(cancellationToken);
        return Ok(new GetTasksResponse(taskDtos));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask([FromRoute] int id, CancellationToken cancellationToken)
    {
        var taskQuery = context.Tasks
            .Where(t => t.Id == id);
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
        var task = await context.Tasks.FindAsync(id, cancellationToken);
        if (task == null)
        {
            return NotFound();
        }

        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutTask([FromRoute] int id, PutTaskRequest request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
        };

        context.Entry(task).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}