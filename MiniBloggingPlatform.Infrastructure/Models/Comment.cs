using System.ComponentModel.DataAnnotations;

namespace MiniBloggingPlatform.Infrastructure.Models;

public class Comment
{
    public int Id { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign keys
    [Required]
    public int PostId { get; set; }
    
    [Required]
    public string AuthorId { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }
    
    // Navigation properties
    public virtual Post? Post { get; set; }
    public virtual ApplicationUser? Author { get; set; }
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}

