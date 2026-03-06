using Moq;
using FluentAssertions;
using MusicPlayer.Application.Services;
using MusicPlayer.Application.Interfaces;
using MusicPlayer.Domain.Interfaces;
using MusicPlayer.Domain.Entities;
using MusicPlayer.Domain.Exceptions;
using MusicPlayer.Application.Helpers;
using Xunit;

namespace MusicPlayer.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtTokenService> _tokenServiceMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<IJwtTokenService>();
        _service = new AuthService(_userRepoMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@user.dk";
        var password = "Password123!";
        var hashedPassword = PasswordHelper.HashPassword(password);

        var user = new User
        {
            Id = 1,
            Email = email,
            PasswordHash = hashedPassword
        };

        _userRepoMock.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateToken(user)).Returns("fake-jwt-token");

        // Act
        var result = await _service.Login(email, password);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        _tokenServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetUserByEmail(It.IsAny<string>()))
            .ReturnsAsync((User)null);

        // Act & Assert
        await _service.Invoking(s => s.Login("wrong@email.dk", "anyPassword"))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task Login_ShouldThrowUnauthorizedException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "test@user.dk";
        var correctPassword = "CorrectPassword123";
        var wrongPassword = "WrongPassword123";
        var hashedPassword = PasswordHelper.HashPassword(correctPassword);

        var user = new User { Id = 1, Email = email, PasswordHash = hashedPassword };

        _userRepoMock.Setup(x => x.GetUserByEmail(email)).ReturnsAsync(user);

        // Act & Assert
        await _service.Invoking(s => s.Login(email, wrongPassword))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password.");
    }
}