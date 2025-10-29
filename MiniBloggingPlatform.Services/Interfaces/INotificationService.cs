using MiniBloggingPlatform.Infrastructure.Models;

namespace MiniBloggingPlatform.Services.Interfaces;

public interface INotificationService
{
	Task CreateAsync(Notification notification);
	Task<int> GetUnreadCountAsync(string userId);
	Task<IEnumerable<Notification>> GetRecentAsync(string userId, int take = 10);
	Task MarkAllReadAsync(string userId);
}
