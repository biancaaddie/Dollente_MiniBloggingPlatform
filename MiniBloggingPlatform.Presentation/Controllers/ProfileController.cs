using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniBloggingPlatform.Infrastructure.Models;

namespace MiniBloggingPlatform.Presentation.Controllers;

[Authorize]
public class ProfileController : Controller
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IWebHostEnvironment _env;

	public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
	{
		_userManager = userManager;
		_env = env;
	}

	[AllowAnonymous]
	[HttpGet("profile/{id}")]
	public async Task<IActionResult> ViewProfile(string id)
	{
		var user = await _userManager.FindByIdAsync(id);
		if (user == null) return NotFound();
		return View("Profile", user);
	}

	[HttpGet("profile/edit")]
	public async Task<IActionResult> Edit()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Challenge();
		return View(user);
	}

	[HttpPost("profile/edit")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(string? firstName, string? lastName, string? bio, IFormFile? profileImage)
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Challenge();

		user.FirstName = firstName ?? user.FirstName;
		user.LastName = lastName ?? user.LastName;
		user.Bio = bio;

		if (profileImage != null && profileImage.Length > 0)
		{
			var uploadsDir = Path.Combine(_env.WebRootPath, "profiles");
			Directory.CreateDirectory(uploadsDir);
			var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
			var filePath = Path.Combine(uploadsDir, fileName);
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await profileImage.CopyToAsync(stream);
			}
			user.ProfileImageUrl = $"/profiles/{fileName}";
		}

		await _userManager.UpdateAsync(user);
		return RedirectToAction("ViewProfile", new { id = user.Id });
	}
}
