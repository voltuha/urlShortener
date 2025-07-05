using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.DTO.Requests;
using UrlShortener.Application.DTO.Responses;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly IUrlService _urlService;
    private readonly ILogger<UrlController> _logger;

    public UrlController(IUrlService urlService, ILogger<UrlController> logger)
    {
        _urlService = urlService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<ActionResult<ShortUrlResponse>> CreateShortUrl([FromBody] ShortUrlRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.OriginalUrl))
            {
                return BadRequest("Original URL is required");
            }

            var result = await _urlService.CreateShortUrl(request);
            return Ok(new { ShortUrl = $"{Request.Scheme}://{Request.Host}/{result.Code}" });                
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating short URL");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpDelete("{shortCode}")]
    public async Task<IActionResult> DeleteShortUrl(string shortCode)
    {
        try
        {
            var deleted = await _urlService.DeleteShortUrl(shortCode);
            if (!deleted)
            {
                return NotFound("Short URL not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting short URL: {ShortCode}", shortCode);
            return StatusCode(500, "Internal server error");
        }
    }
}