using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniBloggingPlatform.Infrastructure.Data;
using MiniBloggingPlatform.Infrastructure.Models;
using MiniBloggingPlatform.Presentation;
using MiniBloggingPlatform.Services.Interfaces;
using MiniBloggingPlatform.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext with In-Memory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BloggingDb"));

// Configure Identity with custom password requirements
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add custom password validator
builder.Services.AddScoped<IPasswordValidator<ApplicationUser>, CustomPasswordValidator>();

// Register services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed initial data
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	DbSeeder.SeedAsync(
		services.GetRequiredService<ApplicationDbContext>(),
		services.GetRequiredService<UserManager<ApplicationUser>>(),
		services.GetRequiredService<RoleManager<IdentityRole>>()
	).GetAwaiter().GetResult();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
