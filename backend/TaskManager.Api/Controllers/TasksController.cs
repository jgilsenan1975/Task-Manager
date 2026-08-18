using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Dtos;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetByProject([FromQuery] int projectId)
    {
        var owned = await _db.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == CurrentUserId());
        if (!owned)
        {
            return NotFound();
        }

        var tasks = await _db.Tasks
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> Create(CreateTaskRequest request)
    {
        var owned = await _db.Projects.AnyAsync(p => p.Id == request.ProjectId && p.OwnerId == CurrentUserId());
        if (!owned)
        {
            return NotFound();
        }

        var task = new TaskItem
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssigneeId = request.AssigneeId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return Ok(ToDto(task));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskItemDto>> UpdateStatus(int id, UpdateTaskStatusRequest request)
    {
        var task = await _db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null || task.Project!.OwnerId != CurrentUserId())
        {
            return NotFound();
        }

        task.Status = request.Status;
        await _db.SaveChangesAsync();

        return Ok(ToDto(task));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == id);
        if (task is null || task.Project!.OwnerId != CurrentUserId())
        {
            return NotFound();
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static TaskItemDto ToDto(TaskItem t) => new(
        t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.Priority, t.DueDate, t.AssigneeId, t.CreatedAt);

    private int CurrentUserId() => int.Parse(User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
