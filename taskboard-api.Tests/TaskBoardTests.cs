using Microsoft.EntityFrameworkCore;
using Xunit;
using TaskStatus = TaskboardApi.Models.TaskStatus;
using TaskboardApi.Data;
using TaskboardApi.Helpers;
using TaskboardApi.Models;

namespace TaskboardApi.Tests;

public class TaskBoardTests
{
    private static AppDbContext NewDb()
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(opt);
    }

    [Fact]
    public void Done_Cannot_Move_Back_To_Todo_Or_InProgress()
    {
        Assert.False(TaskRules.CanMove(TaskStatus.Done, TaskStatus.Todo));
        Assert.False(TaskRules.CanMove(TaskStatus.Done, TaskStatus.InProgress));
        Assert.True(TaskRules.CanMove(TaskStatus.Done, TaskStatus.Done));
    }

    [Fact]
    public void Move_Flow_Todo_InProgress_Done_Is_Allowed()
    {
        Assert.True(TaskRules.CanMove(TaskStatus.Todo, TaskStatus.InProgress));
        Assert.True(TaskRules.CanMove(TaskStatus.InProgress, TaskStatus.Done));
        Assert.True(TaskRules.CanMove(TaskStatus.Todo, TaskStatus.Done));
    }

    [Fact]
    public async Task Comment_Add_Persists_With_Text_And_Timestamp()
    {
        using var db = NewDb();
        db.Projects.Add(new Project { Name = "P" });
        await db.SaveChangesAsync();
        var projectId = db.Projects.Single().Id;
        db.Tasks.Add(new TaskItem { ProjectId = projectId, Title = "T", CreatedById = 1 });
        await db.SaveChangesAsync();
        var taskId = db.Tasks.Single().Id;

        var before = DateTime.UtcNow.AddMinutes(-1);
        db.Comments.Add(new Comment { TaskItemId = taskId, UserId = 1, Text = "hello", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var saved = db.Comments.Single();
        Assert.Equal("hello", saved.Text);
        Assert.True(saved.CreatedAt >= before);
    }

    [Fact]
    public void Delete_Manager_Can_Delete_Any_Task()
    {
        Assert.True(AuthorizationHelper.CanDelete("Manager", userId: 99, createdById: 1));
    }

    [Fact]
    public void Delete_Creator_Member_Can_Delete_Own_Task()
    {
        Assert.True(AuthorizationHelper.CanDelete("Member", userId: 7, createdById: 7));
    }

    [Fact]
    public void Delete_Other_Member_Cannot_Delete()
    {
        Assert.False(AuthorizationHelper.CanDelete("Member", userId: 8, createdById: 7));
    }

    [Fact]
    public void Auth_Password_Hash_Verify_Roundtrip()
    {
        var salt = AuthHelper.GenerateSalt();
        var hash = AuthHelper.HashPassword("Secret123!", salt);
        Assert.True(AuthHelper.Verify("Secret123!", salt, hash));
        Assert.False(AuthHelper.Verify("wrong", salt, hash));
    }

    [Fact]
    public async Task Project_With_Tasks_Persisted_And_Filtered_By_Status()
    {
        using var db = NewDb();
        db.Projects.Add(new Project { Name = "Website" });
        await db.SaveChangesAsync();
        var pid = db.Projects.Single().Id;
        db.Tasks.AddRange(
            new TaskItem { ProjectId = pid, Title = "A", Status = TaskStatus.Todo, CreatedById = 1 },
            new TaskItem { ProjectId = pid, Title = "B", Status = TaskStatus.Done, CreatedById = 1 });
        await db.SaveChangesAsync();

        Assert.Equal(2, db.Tasks.Count(t => t.ProjectId == pid));
        Assert.Single(db.Tasks.Where(t => t.ProjectId == pid && t.Status == TaskStatus.Done));
    }
}
