using Microsoft.AspNetCore.Identity;
using MiniBloggingPlatform.Infrastructure.Data;
using MiniBloggingPlatform.Infrastructure.Models;

namespace MiniBloggingPlatform.Presentation;

public static class DbSeeder
{
	public static async Task SeedAsync(
		ApplicationDbContext context,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole> roleManager)
	{
		await context.Database.EnsureCreatedAsync();

		if (!await roleManager.RoleExistsAsync("User"))
		{
			await roleManager.CreateAsync(new IdentityRole("User"));
		}

		var users = new List<ApplicationUser>
		{
			new ApplicationUser { UserName = "alice@example.com", Email = "alice@example.com", FirstName = "Alice", LastName = "Anderson" },
			new ApplicationUser { UserName = "bob@example.com", Email = "bob@example.com", FirstName = "Bob", LastName = "Brown" }
		};

		string strongPwd = "AA@@@111bbb!"; // ≥2 uppercase, ≥3 digits, ≥3 symbols

		foreach (var u in users)
		{
			var existing = await userManager.FindByEmailAsync(u.Email!);
			if (existing == null)
			{
				await userManager.CreateAsync(u, strongPwd);
				await userManager.AddToRoleAsync(u, "User");
			}
		}

		await context.SaveChangesAsync();

		if (!context.Posts.Any())
		{
			var alice = await userManager.FindByEmailAsync("alice@example.com");
			var bob = await userManager.FindByEmailAsync("bob@example.com");

			var samplePosts = new List<Post>
			{
				new Post { Title = "Welcome to the Mini Blog", Content = "This is a seeded welcome post.", AuthorId = alice!.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-120) },
				new Post { Title = "Second Post", Content = "Another example post with some content.", AuthorId = bob!.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-60) }
			};

			context.Posts.AddRange(samplePosts);
			await context.SaveChangesAsync();

			var firstPost = samplePosts.First();
			context.Comments.AddRange(
				new Comment { Content = "Nice post!", PostId = firstPost.Id, AuthorId = bob!.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-30) },
				new Comment { Content = "Thanks for sharing.", PostId = firstPost.Id, AuthorId = alice!.Id, CreatedAt = DateTime.UtcNow.AddMinutes(-20) }
			);
			await context.SaveChangesAsync();
		}
	}
}
