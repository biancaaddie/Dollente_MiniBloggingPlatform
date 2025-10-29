using System.ComponentModel.DataAnnotations;

namespace MiniBloggingPlatform.Infrastructure.Models;

public class Tag
{
	public int Id { get; set; }
	[Required]
	[StringLength(50)]
	public string Name { get; set; } = string.Empty;
	public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}

public class PostTag
{
	public int PostId { get; set; }
	public int TagId { get; set; }
	public virtual Post? Post { get; set; }
	public virtual Tag? Tag { get; set; }
}
