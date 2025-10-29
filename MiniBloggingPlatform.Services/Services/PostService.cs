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
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
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

    public async Task<Post> CreatePostAsync(Post post, IEnumerable<string>? tagNames = null)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        if (tagNames != null)
        {
            await UpsertPostTagsAsync(post, tagNames);
        }
        return post;
    }

    public async Task UpdatePostAsync(Post post, IEnumerable<string>? tagNames = null)
    {
        var existingPost = await _context.Posts.Include(p => p.PostTags).FirstOrDefaultAsync(p => p.Id == post.Id);
        if (existingPost != null)
        {
            existingPost.Title = post.Title;
            existingPost.Content = post.Content;
            existingPost.ImageUrl = post.ImageUrl;
            existingPost.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (tagNames != null)
            {
                await UpsertPostTagsAsync(existingPost, tagNames);
            }
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

    public async Task<(IEnumerable<Post> items, int totalCount)> GetPagedPostsAsync(string? search, int page, int pageSize, string sort = "date_desc", string? tag = null)
    {
        var query = _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Title.Contains(term) || p.Content.Contains(term) || p.PostTags.Any(pt => pt.Tag!.Name.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(p => p.PostTags.Any(pt => pt.Tag!.Name == tag));
        }

        query = sort switch
        {
            "date_asc" => query.OrderBy(p => p.CreatedAt),
            "author" => query.OrderBy(p => p.Author!.FirstName).ThenBy(p => p.Author!.LastName),
            "popularity" => query.OrderByDescending(p => p.Comments.Count),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<IEnumerable<string>> GetAllTagNamesAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .Select(t => t.Name)
            .ToListAsync();
    }

    private async Task UpsertPostTagsAsync(Post post, IEnumerable<string> tagNames)
    {
        var normalized = tagNames
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingTags = await _context.Tags
            .Where(t => normalized.Contains(t.Name))
            .ToListAsync();

        var toCreate = normalized
            .Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
            .Select(name => new Tag { Name = name })
            .ToList();

        if (toCreate.Count > 0)
        {
            _context.Tags.AddRange(toCreate);
            await _context.SaveChangesAsync();
            existingTags.AddRange(toCreate);
        }

        var existingLinks = await _context.PostTags.Where(pt => pt.PostId == post.Id).ToListAsync();
        if (existingLinks.Count > 0)
        {
            _context.PostTags.RemoveRange(existingLinks);
        }

        var newLinks = existingTags.Select(t => new PostTag { PostId = post.Id, TagId = t.Id });
        _context.PostTags.AddRange(newLinks);
        await _context.SaveChangesAsync();
    }
}

