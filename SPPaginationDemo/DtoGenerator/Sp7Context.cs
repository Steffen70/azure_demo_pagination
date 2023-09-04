using Microsoft.EntityFrameworkCore;
using SPPaginationDemo.CallLogger;

#pragma warning disable IDE0290

namespace SPPaginationDemo.DtoGenerator;

public class Sp7Context<T> : DbContext where T : class
{
    [Log]
    public DbSet<T> GeneratedModel { get; set; } = null!;

    public Sp7Context(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<T>().HasNoKey();

        base.OnModelCreating(builder);
    }
}