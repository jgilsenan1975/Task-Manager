using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Models;

namespace TaskManager.Api.Dtos;

public record CreateTaskRequest(
    [Required] int ProjectId,
    [Required, MaxLength(200)] string Title,
    string? Description,
    TaskItemPriority Priority = TaskItemPriority.Medium,
    DateTime? DueDate = null,
    int? AssigneeId = null
);

public record UpdateTaskStatusRequest([Required] TaskItemStatus Status);

public record TaskItemDto(
    int Id,
    int ProjectId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskItemPriority Priority,
    DateTime? DueDate,
    int? AssigneeId,
    DateTime CreatedAt
);
