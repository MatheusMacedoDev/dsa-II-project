namespace EncurtadorUfabc.Core.Models;

public class ShortUrl
{
    public string Code { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public long AccessCount { get; set; }
}
