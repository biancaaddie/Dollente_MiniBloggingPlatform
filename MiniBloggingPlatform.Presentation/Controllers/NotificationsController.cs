using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MiniBloggingPlatform.Infrastructure.Models;
using MiniBloggingPlatform.Services.Interfaces;

namespace MiniBloggingPlatform.Presentation.Controllers;

[Authorize]
public class NotificationsController : Controller
{
	private readonly INotificationService _notificationService;
	private readonly UserManager<ApplicationUser> _userManager;

	public NotificationsController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
	{
		_notificationService = notificationService;
		_userManager = userManager;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Challenge();
		var items = await _notificationService.GetRecentAsync(user.Id, 20);
		return View(items);
	}

	[HttpGet("notifications/count")]
	public async Task<IActionResult> Count()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Json(new { count = 0 });
		var count = await _notificationService.GetUnreadCountAsync(user.Id);
		return Json(new { count });
	}

	[HttpPost("notifications/mark-all-read")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> MarkAllRead()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Challenge();
		await _notificationService.MarkAllReadAsync(user.Id);
		return RedirectToAction("Index");
	}
}
