namespace UrlShortener.Application.DTO.Responses;

public class ShortUrlResponse
{
    public long Id { get; set; }

    public string OriginalUrl { get; set; }

    public string Code { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}