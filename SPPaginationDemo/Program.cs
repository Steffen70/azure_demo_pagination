using Microsoft.EntityFrameworkCore;
using SPPaginationDemo;
using SPPaginationDemo.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(@"Data Source=sp_pagination_demo.sqlite;"));

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" && File.Exists(Path.Combine(".", "sp_pagination_demo.sqlite")))
{
#pragma warning disable ASP0000
    using var scope = builder.Services.BuildServiceProvider().CreateScope();
#pragma warning restore ASP0000
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();

    // Checking if there is any data already
    if (!context.WeatherForecasts.Any())
    {
        var seedData = new List<WeatherForecast>
        {
            new()
            {
            Date = DateTime.Now.AddDays(1),
            TemperatureC = 25,
            Summary = "Warm"
            },
            new()
            {
                Date = DateTime.Now.AddDays(1).AddHours(5),
                TemperatureC = 19,
                Summary = "Storm Warning"
            },
            new()
            {
                Date = DateTime.Now.AddDays(2),
                TemperatureC = 15,
                Summary = "Chilly"
            },
            new()
            {
                Date = DateTime.Now.AddDays(3),
                TemperatureC = 10,
                Summary = "Cold"
            },
            new()
            {
                Date = DateTime.Now.AddDays(4),
                TemperatureC = 20,
                Summary = "Cool"
            },
            new()
            {
                Date = DateTime.Now.AddDays(5),
                TemperatureC = 30,
                Summary = "Hot"
            },
            new()
            {
                Date = DateTime.Now.AddDays(6),
                TemperatureC = 35,
                Summary = "Very Hot"
            }
        };

        context.Database.Migrate();
        context.WeatherForecasts.AddRange(seedData);
        context.SaveChanges();
    }
}

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
