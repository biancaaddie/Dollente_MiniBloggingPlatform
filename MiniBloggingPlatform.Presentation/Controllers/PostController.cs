using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniBloggingPlatform.Infrastructure.Models;
using MiniBloggingPlatform.Presentation.Models;
using MiniBloggingPlatform.Services.Interfaces;

namespace MiniBloggingPlatform.Presentation.Controllers;

[Authorize]
public class PostController : Controller
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public PostController(IPostService postService, ICommentService commentService, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _postService = postService;
        _commentService = commentService;
        _userManager = userManager;
        _env = env;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 5)
    {
        var (items, totalCount) = await _postService.GetPagedPostsAsync(search, page, pageSize);

        var postsViewModels = items.Select(p => new PostViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            AuthorId = p.AuthorId,
            AuthorName = $"{p.Author?.FirstName} {p.Author?.LastName}".Trim(),
            CommentCount = p.Comments?.Count ?? 0
        }).ToList();

        var model = new PostsIndexViewModel
        {
            Search = search ?? string.Empty,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Posts = postsViewModels
        };

        ViewBag.AllTags = await _postService.GetAllTagNamesAsync();
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var post = new Post
                {
                    Title = model.Title,
                    Content = model.Content,
                    AuthorId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                // image upload
                if (model.Image != null && model.Image.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Image.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }
                    post.ImageUrl = $"/uploads/{fileName}";
                }

                var tagNames = string.IsNullOrWhiteSpace(model.Tags)
                    ? Array.Empty<string>()
                    : model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                await _postService.CreatePostAsync(post, tagNames);
                return RedirectToAction("Index");
            }
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        var postViewModel = new PostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            AuthorId = post.AuthorId,
            AuthorName = $"{post.Author?.FirstName} {post.Author?.LastName}".Trim(),
            CommentCount = post.Comments?.Count ?? 0
        };

        var comments = post.Comments?.Where(c => c.ParentCommentId == null).Select(c => new CommentViewModel
        {
            Id = c.Id,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            PostId = c.PostId,
            AuthorId = c.AuthorId,
            AuthorName = $"{c.Author?.FirstName} {c.Author?.LastName}".Trim()
        }).ToList() ?? new List<CommentViewModel>();

        ViewBag.Post = postViewModel;
        ViewBag.Comments = comments;
        ViewBag.PostImageUrl = post.ImageUrl;
        ViewBag.Tags = post.PostTags.Select(pt => pt.Tag!.Name).ToList();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var post = await _postService.GetPostByIdAsync(id);

        if (post == null)
        {
            return NotFound();
        }

        if (post.AuthorId != user?.Id)
        {
            return Forbid();
        }

        var model = new CreatePostViewModel
        {
            Title = post.Title,
            Content = post.Content,
            Tags = string.Join(", ", post.PostTags.Select(pt => pt.Tag!.Name))
        };

        ViewBag.PostId = id;
        ViewBag.ExistingImage = post.ImageUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreatePostViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            var post = await _postService.GetPostByIdAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            if (post.AuthorId != user?.Id)
            {
                return Forbid();
            }

            post.Title = model.Title;
            post.Content = model.Content;
            post.UpdatedAt = DateTime.UtcNow;

            if (model.Image != null && model.Image.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.Image.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }
                post.ImageUrl = $"/uploads/{fileName}";
            }

            var tagNames = string.IsNullOrWhiteSpace(model.Tags)
                ? Array.Empty<string>()
                : model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            await _postService.UpdatePostAsync(post, tagNames);
            return RedirectToAction("Details", new { id });
        }

        ViewBag.PostId = id;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var post = await _postService.GetPostByIdAsync(id);

        if (post == null)
        {
            return NotFound();
        }

        if (post.AuthorId != user?.Id)
        {
            return Forbid();
        }

        await _postService.DeletePostAsync(id);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int postId, string content, int? parentCommentId)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Json(new { success = false, message = "Comment cannot be empty." });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "User not authenticated." });
        }

        var post = await _postService.GetPostByIdAsync(postId);
        if (post == null)
        {
            return Json(new { success = false, message = "Post not found." });
        }

        var comment = new Comment
        {
            Content = content,
            PostId = postId,
            AuthorId = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        if (parentCommentId.HasValue)
        {
            await _commentService.ReplyToCommentAsync(parentCommentId.Value, comment);
        }
        else
        {
            await _commentService.CreateCommentAsync(comment);
        }

        return Json(new { success = true, message = "Comment added successfully." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var comment = await _commentService.GetCommentByIdAsync(id);

        if (comment == null)
        {
            return Json(new { success = false, message = "Comment not found." });
        }

        if (comment.AuthorId != user?.Id)
        {
            return Json(new { success = false, message = "You don't have permission to delete this comment." });
        }

        await _commentService.DeleteCommentAsync(id);
        return Json(new { success = true, message = "Comment deleted successfully." });
    }
}

