using System.ComponentModel.DataAnnotations;

namespace EncurtadorUfabc.Core.Persistence;

public class ShortUrlEntity
{
    [Key]
    public string Code { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public long AccessCount { get; set; }
}
