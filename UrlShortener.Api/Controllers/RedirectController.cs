using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("/")]
public class RedirectController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly ILogger<RedirectController> _logger;
    
    public RedirectController(IUrlService urlService, ILogger<RedirectController> logger)
    {
        _urlService = urlService;
        _logger = logger;
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
    {
        try
        {
            var shortUrl = await _urlService.GetShortUrl(shortCode);
            if (shortUrl is null)
            {
                return NotFound("Url not found or expired");
            }

            _logger.LogInformation("Redirecting {ShortCode} to {OriginalUrl}", shortCode, shortUrl.OriginalUrl);
            return Redirect(shortUrl.OriginalUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to redirect {ShortCode}", shortCode);
            return StatusCode(500, "Internal server error");
        }
    } 
}