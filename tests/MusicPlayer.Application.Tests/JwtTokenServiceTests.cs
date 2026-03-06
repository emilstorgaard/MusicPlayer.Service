using FluentAssertions;
using MusicPlayer.Application.Configurations;
using MusicPlayer.Application.Services;
using MusicPlayer.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace MusicPlayer.Application.Tests;

public class JwtTokenServiceTests
{
    private readonly Settings _settings;
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        _settings = new Settings
        {
            JwtSecret = "DetteErEnMegetHemmeligOgLangKodeSomSkalBrugesTilTest123!",
            JwtExpiryHours = 2
        };
        _service = new JwtTokenService(_settings);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken_WithCorrectClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 123,
            Email = "test@music.dk",
            PasswordHash = "fake_hash"
        };

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Subject.Should().Be("123");
        jwtToken.Claims.First(c => c.Type == "email").Value.Should().Be("test@music.dk");
    }
}