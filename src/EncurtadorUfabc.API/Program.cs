using EncurtadorUfabc.Core.Crosscutting;
using EncurtadorUfabc.Core.Persistence;
using EncurtadorUfabc.Core.Services;
using EncurtadorUfabc.AVL;
using EncurtadorUfabc.Core.Models;
using EncurtadorUfabc.Hash;
using Microsoft.EntityFrameworkCore;
using EncurtadorUfabc.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IBenchmarkService, BenchmarkService>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=encurtador.db"));

builder.Services.AddKeyedSingleton<ISymbolTable<string, ShortUrl>>("hash", (_, _) => new HashSymbolTable<string, ShortUrl>());
builder.Services.AddKeyedSingleton<ISymbolTable<string, ShortUrl>>("avl", (_, _) => new AvlSymbolTable<string, ShortUrl>());

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new DoubleConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var hashTable = app.Services.GetRequiredKeyedService<ISymbolTable<string, ShortUrl>>("hash");
    var avlTable = app.Services.GetRequiredKeyedService<ISymbolTable<string, ShortUrl>>("avl");

    foreach (var entity in db.ShortUrls.AsNoTracking())
    {
        hashTable.Put(entity.Code, new ShortUrl { Code = entity.Code, OriginalUrl = entity.OriginalUrl, CreatedAt = entity.CreatedAt, AccessCount = entity.AccessCount });
        avlTable.Put(entity.Code, new ShortUrl { Code = entity.Code, OriginalUrl = entity.OriginalUrl, CreatedAt = entity.CreatedAt, AccessCount = entity.AccessCount });
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseEndpoints();

app.Run();
