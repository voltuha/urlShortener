using UrlShortener.Application.DTO.Responses;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Mappings;

public static class ShortUrlMapping
{
    public static ShortUrlResponse MapEntityToResponse(ShortUrl entity)
    {
        return new ShortUrlResponse
        {
            Id = entity.Id,
            OriginalUrl = entity.OriginalUrl,
            Code = entity.Code,
            CreatedAt = entity.CreatedAt,
            ExpiresAt = entity.ExpiresAt,
        };
    }
}