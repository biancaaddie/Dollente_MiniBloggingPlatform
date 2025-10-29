using Microsoft.EntityFrameworkCore;
using MiniBloggingPlatform.Infrastructure.Data;
using MiniBloggingPlatform.Infrastructure.Models;
using MiniBloggingPlatform.Services.Interfaces;

namespace MiniBloggingPlatform.Services.Services;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync()
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetPostsByAuthorIdAsync(string authorId)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == authorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task UpdatePostAsync(Post post)
    {
        var existingPost = await _context.Posts.FindAsync(post.Id);
        if (existingPost != null)
        {
            existingPost.Title = post.Title;
            existingPost.Content = post.Content;
            existingPost.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeletePostAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> PostExistsAsync(int id)
    {
        return await _context.Posts.AnyAsync(p => p.Id == id);
    }
}

