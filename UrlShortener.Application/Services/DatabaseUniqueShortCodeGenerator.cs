using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Configuration;
using UrlShortener.Application.Interfaces;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.Application.Services;

public class DatabaseUniqueShortCodeGenerator : IShortCodeGenerator
{
    private readonly UrlShortenerContext _context;
    private readonly string _allowedChars;
    private readonly int _length;

    public DatabaseUniqueShortCodeGenerator(UrlShortenerContext context,
        IOptions<ShortCodeGenerationOptions> options)
    {
        _context = context;
        _allowedChars = options.Value.AllowedChars;
        _length = options.Value.Length;
    }
    
    public async ValueTask<string> GenerateShortCode()
    {
        const int maxAttempts = 100;
        var random = new Random();
        var length = _length; 

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var id = GenerateRandomId(random, length);
                
            if (!await _context.ShortUrls.AnyAsync(x => x.Code == id))
            {
                return id;
            }
                
            if (attempt > 50)
            {
                length = _length + 2;
            }
        }
            
        // Fallback to GUID if we can't generate a unique short id
        return Guid.NewGuid().ToString("N")[..8];
    }
    
    private string GenerateRandomId(Random random, int length)
    {
        var result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            result.Append(_allowedChars[random.Next(_allowedChars.Length)]);
        }
        return result.ToString();
    }
}