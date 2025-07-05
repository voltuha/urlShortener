using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using UrlShortener.Application.Configuration;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Application.Services;

public class RandomBase62IdGenerator : IShortCodeGenerator
{
    private readonly string _allowedChars;
    private readonly int _length;

    public RandomBase62IdGenerator(IOptions<ShortCodeGenerationOptions> options)
    {
        _allowedChars = options.Value.AllowedChars;
        _length = options.Value.Length;
    }

    public ValueTask<string> GenerateShortCode()
    {
        var id = GenerateCode();
        return ValueTask.FromResult(id);
    }

    private string GenerateCode()
    {
        var result = new StringBuilder(_length);
        var bytes = new byte[_length];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        foreach (var b in bytes)
        {
            result.Append(_allowedChars[b % _allowedChars.Length]);
        }

        return result.ToString();
    }
}