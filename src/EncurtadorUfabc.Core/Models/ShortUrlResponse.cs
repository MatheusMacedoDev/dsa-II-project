namespace EncurtadorUfabc.Core.Models;

public record ShortUrlResponse(string Code, string OriginalUrl, DateTimeOffset CreatedAt, long AccessCount);
