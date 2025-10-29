using System.ComponentModel.DataAnnotations;

namespace MiniBloggingPlatform.Presentation.Models;

public class CommentViewModel
{
    public int Id { get; set; }
    
    [Required]
    [Display(Name = "Comment")]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int PostId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
}

