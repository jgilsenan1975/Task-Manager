using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Dtos;

public record RegisterRequest(
    [Required, MinLength(2)] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record UserDto(int Id, string DisplayName, string Email);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);
