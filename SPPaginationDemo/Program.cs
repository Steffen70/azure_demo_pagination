using SPPaginationDemo.Controllers;
using SPPaginationDemo.Filtration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

builder.Services.AddControllers();

var app = builder.Build();

await LiveUpdateController.LoadAllUpdateAssemblies();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
