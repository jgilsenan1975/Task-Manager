namespace TaskManager.Api.Models;

public class TaskComment
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public int AuthorId { get; set; }
    public AppUser? Author { get; set; }
}
