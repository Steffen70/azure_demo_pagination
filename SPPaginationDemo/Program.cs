using SPPaginationDemo.CallLogger;
using SPPaginationDemo.Filtration;
using SPPaginationDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

builder.Services.AddControllers();

builder.Services.AddSingleton(builder.Configuration);

builder.Services.AddSingleton<Appsettings>();

builder.Services.AddSingleton<ILogger, ApiLogger>();

builder.Services.AddSingleton<ExceptionMiddleware>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger>();
logger.LogInformation("Application started.");

// Redirect console output to logger
Console.SetOut(new LoggerTextWriter(logger));

// Configure the HTTP request pipeline.

// Todo: DS: Remove exception page in production
app.UseDeveloperExceptionPage();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
