namespace TaskManager.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
    public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ProjectId { get; set; }
    public ProjectEntity? Project { get; set; }

    public int? AssigneeId { get; set; }
    public AppUser? Assignee { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
