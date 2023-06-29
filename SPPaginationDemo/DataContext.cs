using Microsoft.EntityFrameworkCore;
using SPPaginationDemo.Model;

namespace SPPaginationDemo;

public class DataContext : DbContext
{
    public DbSet<WeatherForecast> WeatherForecasts { get; set; } = null!;

    public DataContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}