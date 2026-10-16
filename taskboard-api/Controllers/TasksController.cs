using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskboardApi.Data;
using TaskboardApi.Helpers;
using TaskboardApi.Models;

namespace TaskboardApi.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentRole => User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

    [HttpPost("{id:int}/move")]
    public async Task<IActionResult> Move(int id, [FromBody] MoveTaskRequest req)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!TaskRules.TryParseStatus(req.Status, out var target))
            return BadRequest("Invalid status. Use Todo, InProgress or Done.");
        if (!TaskRules.CanMove(task.Status, target))
            return BadRequest("Done tasks cannot be moved back.");
        task.Status = target;
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int id)
    {
        if (!await _db.Tasks.AnyAsync(t => t.Id == id))
            return NotFound("Task not found.");
        var comments = await _db.Comments.Where(c => c.TaskItemId == id)
            .OrderBy(c => c.CreatedAt).AsNoTracking().ToListAsync();
        return Ok(comments);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<Comment>> AddComment(int id, [FromBody] CreateCommentRequest req)
    {
        if (!await _db.Tasks.AnyAsync(t => t.Id == id))
            return NotFound("Task not found.");
        if (string.IsNullOrWhiteSpace(req.Text))
            return BadRequest("Text is required.");
        var comment = new Comment
        {
            TaskItemId = id,
            UserId = CurrentUserId,
            Text = req.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetComments), new { id }, comment);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!AuthorizationHelper.CanDelete(CurrentRole, CurrentUserId, task.CreatedById))
            return Forbid();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
