using MiniBloggingPlatform.Infrastructure.Models;

namespace MiniBloggingPlatform.Services.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId);
    Task<Comment?> GetCommentByIdAsync(int id);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task UpdateCommentAsync(Comment comment);
    Task DeleteCommentAsync(int id);
}

