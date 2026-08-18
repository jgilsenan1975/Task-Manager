namespace TaskManager.Api.Models;

public class AppUser
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectEntity> OwnedProjects { get; set; } = new List<ProjectEntity>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
