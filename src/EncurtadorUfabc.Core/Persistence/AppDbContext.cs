using Microsoft.EntityFrameworkCore;

namespace EncurtadorUfabc.Core.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrlEntity> ShortUrls => Set<ShortUrlEntity>();
}
