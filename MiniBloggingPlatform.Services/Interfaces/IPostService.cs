using MiniBloggingPlatform.Infrastructure.Models;

namespace MiniBloggingPlatform.Services.Interfaces;

public interface IPostService
{
    Task<IEnumerable<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(int id);
    Task<IEnumerable<Post>> GetPostsByAuthorIdAsync(string authorId);
    Task<Post> CreatePostAsync(Post post, IEnumerable<string>? tagNames = null);
    Task UpdatePostAsync(Post post, IEnumerable<string>? tagNames = null);
    Task DeletePostAsync(int id);
    Task<bool> PostExistsAsync(int id);

    Task<(IEnumerable<Post> items, int totalCount)> GetPagedPostsAsync(string? search, int page, int pageSize);
    Task<IEnumerable<string>> GetAllTagNamesAsync();
}

