using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Application.Services;

public class ExpiredUrlCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredUrlCleanupService> _logger;

    public ExpiredUrlCleanupService(IServiceProvider serviceProvider, ILogger<ExpiredUrlCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<UrlShortenerContext>();

                var expiredUrls = await context.ShortUrls
                    .Where(x => x.ExpiresAt.HasValue && x.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                if (expiredUrls.Count > 0)
                {
                    context.ShortUrls.RemoveRange(expiredUrls);
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Cleaned up {Count} expired URLs", expiredUrls.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during expired URL cleanup");
            }

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}