using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Dtos;

public record CreateProjectRequest(
    [Required, MaxLength(120)] string Name,
    string? Description
);

public record ProjectDto(int Id, string Name, string? Description, DateTime CreatedAt, int OwnerId);
