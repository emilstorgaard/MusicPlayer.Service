using FluentAssertions;
using Moq;
using MusicPlayer.Application.Dtos.Request;
using MusicPlayer.Application.Services;
using MusicPlayer.Domain.Entities;
using MusicPlayer.Domain.Exceptions;
using MusicPlayer.Domain.Interfaces;
using Xunit;

namespace MusicPlayer.Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPlaylistRepository> _playlistRepoMock;
    private readonly Mock<ISongRepository> _songRepoMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _playlistRepoMock = new Mock<IPlaylistRepository>();
        _songRepoMock = new Mock<ISongRepository>();

        _service = new UserService(
            _userRepoMock.Object,
            _playlistRepoMock.Object,
            _songRepoMock.Object);
    }

    [Fact]
    public async Task AddUser_ShouldHashPasswordAndSaveUser_WhenEmailIsUnique()
    {
        // Arrange
        var dto = new UserReqDto { Email = "new@user.dk", Password = "MySecretPassword123" };

        _userRepoMock.Setup(x => x.GetUserByEmail(dto.Email))
            .ReturnsAsync((User)null);

        // Act
        await _service.AddUser(dto);

        // Assert
        _userRepoMock.Verify(x => x.AddUser(It.Is<User>(u =>
            u.Email == dto.Email &&
            u.PasswordHash != dto.Password
        )), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldRemoveLikesAndUser_WhenUserExists()
    {
        // Arrange
        int userId = 10;
        var user = new User { Id = userId, Email = "delete@me.dk", PasswordHash = "x" };

        _userRepoMock.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);

        // Act
        await _service.Delete(userId);

        // Assert
        _playlistRepoMock.Verify(x => x.DeleteLikedPlaylists(userId), Times.Once);
        _songRepoMock.Verify(x => x.DeleteLikedSongs(userId), Times.Once);
        _userRepoMock.Verify(x => x.Delete(user), Times.Once);
    }

    [Fact]
    public async Task AddUser_ShouldThrowConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var dto = new UserReqDto { Email = "exists@test.dk", Password = "123" };
        _userRepoMock.Setup(x => x.GetUserByEmail(dto.Email))
            .ReturnsAsync(new User { Email = dto.Email, PasswordHash = "..." });

        // Act & Assert
        await _service.Invoking(s => s.AddUser(dto))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task GetUser_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetUserById(It.IsAny<int>())).ReturnsAsync((User)null);

        // Act & Assert
        await _service.Invoking(s => s.GetUser(1))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAll_ShouldThrowNotFoundException_WhenNoUsersExist()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetAllUsers())
            .ReturnsAsync(new List<User>());

        // Act & Assert
        await _service.Invoking(s => s.GetAll())
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("No users found.");
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfDtos_WhenUsersExist()
    {
        // Arrange
        var users = new List<User>
    {
        new User { Id = 1, Email = "u1@test.com", PasswordHash = "x" },
        new User { Id = 2, Email = "u2@test.com", PasswordHash = "y" }
    };
        _userRepoMock.Setup(x => x.GetAllUsers()).ReturnsAsync(users);

        // Act
        var result = await _service.GetAll();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Email == "u1@test.com");
    }
}