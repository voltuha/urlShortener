using UrlShortener.Application.Enums;

namespace UrlShortener.Application.Configuration;

public class ShortCodeGenerationOptions
{
    public string AllowedChars { get; set; }
    public int Length { get; set; }
    public ShortCodeGenerationStrategy Strategy { get; set; }
}