using Microsoft.EntityFrameworkCore;
using MiniBloggingPlatform.Infrastructure.Data;
using MiniBloggingPlatform.Infrastructure.Models;
using MiniBloggingPlatform.Services.Interfaces;

namespace MiniBloggingPlatform.Services.Services;

public class NotificationService : INotificationService
{
	private readonly ApplicationDbContext _context;

	public NotificationService(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task CreateAsync(Notification notification)
	{
		_context.Notifications.Add(notification);
		await _context.SaveChangesAsync();
	}

	public async Task<int> GetUnreadCountAsync(string userId)
	{
		return await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).CountAsync();
	}

	public async Task<IEnumerable<Notification>> GetRecentAsync(string userId, int take = 10)
	{
		return await _context.Notifications
			.Where(n => n.UserId == userId)
			.OrderByDescending(n => n.CreatedAt)
			.Take(take)
			.ToListAsync();
	}

	public async Task MarkAllReadAsync(string userId)
	{
		var unread = await _context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
		if (unread.Count > 0)
		{
			foreach (var n in unread) n.IsRead = true;
			await _context.SaveChangesAsync();
		}
	}
}
