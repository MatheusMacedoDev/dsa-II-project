using EncurtadorUfabc.Core.Contracts;
using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace EncurtadorUfabc.API.Endpoints.Hash.CreateUrl;

public class CreateUrlEndpoint : IEndpoint
{
    private const string CodeAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const int CodeLength = 7;

    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/hash/urls", CreateAsync).WithTags("Hash").WithSummary("Cria uma URL curta usando a tabela hash").AddEndpointFilter(new LogProcessFilter("Hash.CreateUrl"));
    }

    public async Task<IResult> CreateAsync([FromBody] CreateUrlRequest request, [FromKeyedServices("hash")] ISymbolTable<string, ShortUrl> table, AppDbContext db, HttpContext httpContext, ILogger<CreateUrlEndpoint> logger, CancellationToken ct)
    {
        if (!IsValidUrl(request.OriginalUrl))
            return Results.BadRequest("A URL informada e invalida.");

        var code = GenerateUniqueCode(table);
        var shortUrl = new ShortUrl { Code = code, OriginalUrl = request.OriginalUrl, CreatedAt = DateTimeOffset.UtcNow, AccessCount = 0 };

        table.Put(code, shortUrl);
        db.ShortUrls.Add(new ShortUrlEntity { Code = code, OriginalUrl = shortUrl.OriginalUrl, CreatedAt = shortUrl.CreatedAt, AccessCount = shortUrl.AccessCount });
        await db.SaveChangesAsync(ct);

        var shortUrlText = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/hash/urls/{code}";
        return Results.Created(shortUrlText, new CreateUrlResponse(code, shortUrlText, shortUrl.OriginalUrl));
    }

    private static bool IsValidUrl(string value) => !string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static string GenerateUniqueCode(ISymbolTable<string, ShortUrl> table)
    {
        string code;
        do
        {
            code = GenerateCode();
        }
        while (table.Contains(code));
        return code;
    }

    private static string GenerateCode()
    {
        var characters = new char[CodeLength];
        for (int index = 0; index < CodeLength; index++)
            characters[index] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        return new string(characters);
    }
}
