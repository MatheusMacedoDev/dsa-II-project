namespace EncurtadorUfabc.Core.Contracts;

public record ShortUrlResponse(string Code, string OriginalUrl, DateTimeOffset CreatedAt, long AccessCount);
