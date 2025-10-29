namespace MiniBloggingPlatform.Presentation.Models;

public class PostsIndexViewModel
{
	public string Search { get; set; } = string.Empty;
	public string Sort { get; set; } = "date_desc"; // date_desc, date_asc, author, popularity
	public string? Tag { get; set; }
	public IEnumerable<string> AllTags { get; set; } = Enumerable.Empty<string>();
	public int Page { get; set; }
	public int PageSize { get; set; }
	public int TotalCount { get; set; }
	public IEnumerable<PostViewModel> Posts { get; set; } = Enumerable.Empty<PostViewModel>();

	public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
}
