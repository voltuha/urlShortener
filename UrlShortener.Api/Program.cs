using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using SQLitePCL;
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