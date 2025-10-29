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

    public PostController(IPostService postService, ICommentService commentService, UserManager<ApplicationUser> userManager)
    {
        _postService = postService;
        _commentService = commentService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var posts = await _postService.GetAllPostsAsync();
        var postsViewModels = posts.Select(p => new PostViewModel
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

        return View(postsViewModels);
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

                await _postService.CreatePostAsync(post);
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

        var comments = post.Comments?.Select(c => new CommentViewModel
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
            Content = post.Content
        };

        ViewBag.PostId = id;
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

            await _postService.UpdatePostAsync(post);
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
    public async Task<IActionResult> AddComment(int postId, string content)
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

        await _commentService.CreateCommentAsync(comment);

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

