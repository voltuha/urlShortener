using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UrlShortener.Application.DTO.Requests;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure.Persistence;

namespace UrlShortener.UnitTests;

public class UrlServiceTests
{
    private readonly UrlShortenerContext _context;
    private readonly Mock<ILogger<UrlService>> _loggerMock;
    private readonly UrlService _service;

    public UrlServiceTests()
    {
        var options = new DbContextOptionsBuilder<UrlShortenerContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new UrlShortenerContext(options);
        _loggerMock = new Mock<ILogger<UrlService>>();
        _service = new UrlService(_loggerMock.Object, _context);
    }

    [Fact]
    public async Task CreateShortUrl_Should_Create_ShortUrl()
    {
        var request = new ShortUrlRequest { OriginalUrl = "https://example.com" };

        var result = await _service.CreateShortUrl(request);

        Assert.NotNull(result);
        Assert.Equal("https://example.com", result.OriginalUrl);
        Assert.False(string.IsNullOrWhiteSpace(result.Code));
    }

    [Fact]
    public async Task CreateShortUrl_Should_Throw_On_Invalid_Url()
    {
        var request = new ShortUrlRequest { OriginalUrl = "not-a-url" };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateShortUrl(request));
    }

    [Fact]
    public async Task CreateShortUrl_Should_Use_CustomCode()
    {
        var request = new ShortUrlRequest
        {
            OriginalUrl = "https://example.com",
            CustomCode = "mycode"
        };

        var result = await _service.CreateShortUrl(request);

        Assert.Equal("mycode", result.Code);
    }

    [Fact]
    public async Task GetShortUrl_Should_Return_Url()
    {
        var request = new ShortUrlRequest { OriginalUrl = "https://test.com", CustomCode = "get123" };
        await _service.CreateShortUrl(request);

        var result = await _service.GetShortUrl("get123");

        Assert.NotNull(result);
        Assert.Equal("https://test.com", result.OriginalUrl);
    }

    [Fact]
    public async Task GetShortUrl_Should_Return_Null_If_Not_Found()
    {
        var result = await _service.GetShortUrl("doesnotexist");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteShortUrl_Should_Remove_Entry()
    {
        var request = new ShortUrlRequest { OriginalUrl = "https://to-delete.com", CustomCode = "del123" };
        await _service.CreateShortUrl(request);

        var deleted = await _service.DeleteShortUrl("del123");

        Assert.True(deleted);
        Assert.Null(await _service.GetShortUrl("del123"));
    }

    [Fact]
    public async Task DeleteShortUrl_Should_Return_False_If_Not_Found()
    {
        var result = await _service.DeleteShortUrl("unknowncode");
        Assert.False(result);
    }

    [Fact]
    public async Task CreateShortUrl_Should_Throw_On_Duplicate_CustomCode()
    {
        var request = new ShortUrlRequest { OriginalUrl = "https://dup.com", CustomCode = "dupcode" };
        await _service.CreateShortUrl(request);

        var duplicateRequest = new ShortUrlRequest { OriginalUrl = "https://other.com", CustomCode = "dupcode" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateShortUrl(duplicateRequest));
    }
}
