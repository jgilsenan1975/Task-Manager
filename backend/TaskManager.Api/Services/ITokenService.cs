using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(AppUser user);
}
