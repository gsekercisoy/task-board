using TaskboardApi.Models;

namespace TaskboardApi.Models;

public record RegisterRequest(string Username, string Password, string? Role);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, string Username, string Role);
public record CreateProjectRequest(string Name, string? Description);
public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority = TaskPriority.Medium,
    int? AssigneeId = null,
    DateTime? DueDate = null);
public record MoveTaskRequest(string Status);
public record CreateCommentRequest(string Text);
