using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MiniBloggingPlatform.Presentation.Models;

public class CreatePostViewModel
{
    [Required]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Tags (comma-separated)")]
    public string Tags { get; set; } = string.Empty;

    [Display(Name = "Image")]
    public IFormFile? Image { get; set; }
}

