namespace TaskManager.Api.Models;

public class ProjectEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OwnerId { get; set; }
    public AppUser? Owner { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
