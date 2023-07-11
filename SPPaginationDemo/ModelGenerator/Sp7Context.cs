using Microsoft.EntityFrameworkCore;

namespace SPPaginationDemo.ModelGenerator;

public class Sp7Context<T> : DbContext where T : class
{
    public DbSet<T> GeneratedModel { get; set; } = null!;

    public Sp7Context(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<T>().HasNoKey();

        base.OnModelCreating(builder);
    }
}