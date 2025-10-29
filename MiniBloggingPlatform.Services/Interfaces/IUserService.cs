using MiniBloggingPlatform.Infrastructure.Models;

namespace MiniBloggingPlatform.Services.Interfaces;

public interface IUserService
{
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
}

