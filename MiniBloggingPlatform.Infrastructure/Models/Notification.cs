namespace MiniBloggingPlatform.Infrastructure.Models;

public class Notification
{
	public int Id { get; set; }
	public string UserId { get; set; } = string.Empty; // recipient
	public int PostId { get; set; }
	public int? CommentId { get; set; }
	public string Message { get; set; } = string.Empty;
	public bool IsRead { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public virtual ApplicationUser? User { get; set; }
}
