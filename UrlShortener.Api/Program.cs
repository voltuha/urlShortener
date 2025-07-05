using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using SQLitePCL;
using UrlShortener.Application.Configuration;
using UrlShortener.Application.Enums;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure.Persistence;

//Sqlite init
Batteries.Init();

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/urlshortener-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "URL Shortener API", 
        Version = "v1",
        Description = "A REST API for shortening URLs with TTL support"
    });
});

builder.Services.AddDbContext<UrlShortenerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                      "Data Source=urlshortener.db",
        b => b.MigrationsAssembly(typeof(UrlShortenerContext).Assembly.FullName)));

builder.Services.AddOptions();
builder.Services.Configure<ShortCodeGenerationOptions>(
    builder.Configuration.GetSection("ShortCodeGeneration"));

builder.Services.AddScoped(ResolveShortCodeGenerator);

builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddHostedService<ExpiredUrlCleanupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();
    context.Database.Migrate();
}

app.Run();

IShortCodeGenerator ResolveShortCodeGenerator(IServiceProvider serviceProvider)
{
    var options = serviceProvider.GetRequiredService<IOptions<ShortCodeGenerationOptions>>().Value;
    if (!Enum.TryParse(options.Strategy.ToString(), true, out ShortCodeGenerationStrategy strategy))
    {
        strategy = ShortCodeGenerationStrategy.Random;
        serviceProvider.GetRequiredService<ILogger<IShortCodeGenerator>>()
            .LogWarning("Invalid ShortCodeGenerationStrategy '{Strategy}' found in configuration. Defaulting to Random.", options.Strategy);
    }

    return strategy switch
    {
        ShortCodeGenerationStrategy.Random => new RandomBase62IdGenerator(
            serviceProvider.GetRequiredService<IOptions<ShortCodeGenerationOptions>>()),
        ShortCodeGenerationStrategy.Database => new DatabaseUniqueShortCodeGenerator(
            serviceProvider.GetRequiredService<UrlShortenerContext>(),
            serviceProvider.GetRequiredService<IOptions<ShortCodeGenerationOptions>>()),
        _ => throw new ArgumentOutOfRangeException(nameof(options.Strategy), options.Strategy, "Unknown short code generation strategy.")
    };
}