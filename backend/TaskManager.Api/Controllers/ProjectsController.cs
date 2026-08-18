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
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
    {
        var userId = CurrentUserId();

        var projects = await _db.Projects
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto(p.Id, p.Name, p.Description, p.CreatedAt, p.OwnerId))
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetById(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project is null || project.OwnerId != CurrentUserId())
        {
            return NotFound();
        }

        return Ok(new ProjectDto(project.Id, project.Name, project.Description, project.CreatedAt, project.OwnerId));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectRequest request)
    {
        var project = new ProjectEntity
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = CurrentUserId()
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var dto = new ProjectDto(project.Id, project.Name, project.Description, project.CreatedAt, project.OwnerId);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project is null || project.OwnerId != CurrentUserId())
        {
            return NotFound();
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private int CurrentUserId() => int.Parse(User.FindFirst("sub")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
