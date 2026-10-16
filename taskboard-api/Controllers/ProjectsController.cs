using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskboardApi.Data;
using TaskboardApi.Models;

namespace TaskboardApi.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProjectsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        => Ok(await _db.Projects.AsNoTracking().ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Project>> GetOne(int id)
    {
        var project = await _db.Projects.Include(p => p.Tasks)
            .AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
        if (project is null) return NotFound();
        return Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<Project>> Create([FromBody] CreateProjectRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required.");
        var project = new Project { Name = req.Name.Trim(), Description = req.Description };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOne), new { id = project.Id }, project);
    }

    [HttpGet("{id:int}/tasks")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks(int id, [FromQuery] string? status)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == id))
            return NotFound("Project not found.");
        var query = _db.Tasks.Where(t => t.ProjectId == id).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Helpers.TaskRules.TryParseStatus(status, out var parsed))
                return BadRequest("Invalid status. Use Todo, InProgress or Done.");
            query = query.Where(t => t.Status == parsed);
        }
        return Ok(await query.ToListAsync());
    }

    [HttpPost("{id:int}/tasks")]
    public async Task<ActionResult<TaskItem>> CreateTask(int id, [FromBody] CreateTaskRequest req)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == id))
            return NotFound("Project not found.");
        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest("Title is required.");
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var task = new TaskItem
        {
            ProjectId = id,
            Title = req.Title.Trim(),
            Description = req.Description,
            Priority = req.Priority,
            AssigneeId = req.AssigneeId,
            DueDate = req.DueDate,
            Status = Models.TaskStatus.Todo,
            CreatedById = userId
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTasks), new { id }, task);
    }
}
