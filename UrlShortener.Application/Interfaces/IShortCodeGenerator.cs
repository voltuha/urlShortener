namespace UrlShortener.Application.Interfaces;

public interface IShortCodeGenerator
{
    ValueTask<string> GenerateShortCode();
}