namespace UrlShortener.Application.DTO.Requests;

public class ShortUrlRequest
{
    public string OriginalUrl { get; set; }

    public long? TtlSeconds { get; set; }

    public string? CustomCode { get; set; }
}