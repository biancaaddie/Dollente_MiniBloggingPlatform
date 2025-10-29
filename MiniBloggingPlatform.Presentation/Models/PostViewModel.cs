using System.ComponentModel.DataAnnotations;

namespace MiniBloggingPlatform.Presentation.Models;

public class PostViewModel
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CommentCount { get; set; }
}

