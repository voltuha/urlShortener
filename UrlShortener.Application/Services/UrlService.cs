using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UrlShortener.Application.DTO.Requests;
using UrlShortener.Application.DTO.Responses;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Mappings;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Application.Services;

public class UrlService : IUrlService
{
    private readonly ILogger<UrlService> _logger;
    private readonly UrlShortenerContext _context;
    private readonly IShortCodeGenerator _shortCodeGenerator;

    public UrlService(ILogger<UrlService> logger, UrlShortenerContext context, IShortCodeGenerator shortCodeGenerator)
    {
        _logger = logger;
        _context = context;
        _shortCodeGenerator = shortCodeGenerator;
    }

    public async Task<ShortUrlResponse> CreateShortUrl(ShortUrlRequest request)
    {
        _logger.LogInformation("Creating new shortUrl for: {OriginalUrl}", request.OriginalUrl);
        ValidateUrl(request);
        var shortCode = await ResolveShortCode(request);
        
        var shortUrl = new ShortUrl
        {
            Code = shortCode,
            OriginalUrl = request.OriginalUrl,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.TtlSeconds.HasValue 
                ? DateTime.UtcNow.AddSeconds(request.TtlSeconds.Value) 
                : null
        };

        _context.ShortUrls.Add(shortUrl);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Short URL created successfully: {ShortCode} -> {OriginalUrl}", shortCode, request.OriginalUrl);
        return ShortUrlMapping.MapEntityToResponse(shortUrl);
    }
    
    public async Task<ShortUrlResponse> GetShortUrl(string shortCode)
    {
        _logger.LogInformation("Retrieving short URL: {ShortCode}", shortCode);
        var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Code == shortCode);
            
        if (shortUrl == null)
        {
            _logger.LogWarning("Short URL not found: {ShortCode}", shortCode);
            return null;
        }

        if (shortUrl.ExpiresAt.HasValue && shortUrl.ExpiresAt.Value <= DateTime.UtcNow)
        {
            _logger.LogInformation("Short URL expired, removing: {ShortCode}", shortCode);
            _context.ShortUrls.Remove(shortUrl);
            await _context.SaveChangesAsync();
            return null;
        }
        
        _logger.LogInformation("Short URL accessed: {ShortCode} -> {OriginalUrl}", 
            shortCode, shortUrl.OriginalUrl);

        return ShortUrlMapping.MapEntityToResponse(shortUrl);
    }

    public async Task<bool> DeleteShortUrl(string shortCode)
    {
        _logger.LogInformation("Deleting short URL: {shortCode}", shortCode);

        var shortUrl = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Code == shortCode);
        if (shortUrl == null)
        {
            _logger.LogWarning("Short URL not found for deletion: {ShortCode}", shortCode);
            return false;
        }

        _context.ShortUrls.Remove(shortUrl);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Short URL deleted successfully: {ShortCode}", shortCode);
        return true;
    }
    
    private void ValidateUrl(ShortUrlRequest request)
    {
        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
        {
            _logger.LogWarning("Original url is not valid: {OriginalUrl}", request.OriginalUrl);
            throw new ArgumentException("Original url is not valid");
        }
    }
    
    private async Task<string> ResolveShortCode(ShortUrlRequest request)
    {
        string shortCode;
        if (!string.IsNullOrWhiteSpace(request.CustomCode))
        {
            if (!request.CustomCode.All(char.IsLetterOrDigit))
            {
                throw new ArgumentException("Custom id must contain only alphanumeric characters");
            }

            if (await _context.ShortUrls.AnyAsync(x => x.Code == request.CustomCode))
            {
                _logger.LogWarning("Custom id already exists: {CustomCode}", request.CustomCode);
                throw new InvalidOperationException("Custom id already exists");
            }
            
            shortCode = request.CustomCode;
        }
        else
        {
            shortCode = await _shortCodeGenerator.GenerateShortCode();
        }

        return shortCode;
    }
}