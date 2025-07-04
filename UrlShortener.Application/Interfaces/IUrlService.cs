using UrlShortener.Application.DTO.Requests;
using UrlShortener.Application.DTO.Responses;

namespace UrlShortener.Application.Interfaces;

public interface IUrlService
{
    Task<ShortUrlResponse> CreateShortUrl(ShortUrlRequest request);
    
    Task<ShortUrlResponse> GetShortUrl(string shortCode);
    
    Task<bool> DeleteShortUrl(string shortCode);
}