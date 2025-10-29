using System.ComponentModel.DataAnnotations;

namespace MiniBloggingPlatform.Infrastructure.Models;

public class Post
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign key
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    // Navigation property
    public virtual ApplicationUser? Author { get; set; }
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}

