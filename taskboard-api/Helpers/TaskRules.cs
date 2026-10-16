using TaskboardApi.Models;

namespace TaskboardApi.Helpers;

public static class TaskRules
{
    /// <summary>Done tasks are terminal: they cannot move back to any other status.</summary>
    public static bool CanMove(Models.TaskStatus from, Models.TaskStatus to)
    {
        if (from == Models.TaskStatus.Done && to != Models.TaskStatus.Done)
            return false;
        return true;
    }

    public static bool TryParseStatus(string? raw, out Models.TaskStatus status)
        => Enum.TryParse<Models.TaskStatus>(raw, ignoreCase: true, out status)
           && Enum.IsDefined(typeof(Models.TaskStatus), status);
}

public static class AuthorizationHelper
{
    public static bool CanDelete(string? role, int userId, int createdById)
    {
        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            return true;
        return userId == createdById;
    }
}
