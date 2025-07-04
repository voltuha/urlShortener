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

    [HttpGet("{id}")]
    public async Task<IActionResult> RedirectToOriginalUrl(string id)
    {
        try
        {
            var shortUrl = await _urlService.GetShortUrl(id);
            if (shortUrl is null)
            {
                return NotFound("Url not found or expired");
            }

            _logger.LogInformation("Redirecting {Id} to {OriginalUrl}", id, shortUrl.OriginalUrl);
            return Redirect(shortUrl.OriginalUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to redirect {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    } 
}