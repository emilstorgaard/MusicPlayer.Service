using FluentAssertions;
using Moq;
using MusicPlayer.Application.Configurations;
using MusicPlayer.Application.Dtos.Request;
using MusicPlayer.Application.Services;
using MusicPlayer.Domain.Entities;
using MusicPlayer.Domain.Exceptions;
using MusicPlayer.Domain.Interfaces;
using Xunit;

namespace MusicPlayer.Application.Tests;

public class PlaylistServiceTests
{
    private readonly Mock<IPlaylistRepository> _playlistRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ISongRepository> _songRepoMock;
    private readonly Settings _settings;
    private readonly PlaylistService _service;

    public PlaylistServiceTests()
    {
        _playlistRepoMock = new Mock<IPlaylistRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _songRepoMock = new Mock<ISongRepository>();

        _settings = new Settings
        {
            ImageFolder = "TestImages",
            AllowedImageExtensions = new[] { ".jpg", ".png" }
        };

        _service = new PlaylistService(
            _settings,
            _playlistRepoMock.Object,
            _userRepoMock.Object,
            _songRepoMock.Object);
    }

    [Fact]
    public async Task Delete_ShouldThrowUnauthorizedException_WhenUserIsNotOwner()
    {
        // Arrange
        int playlistId = 1;
        int trueOwnerId = 99;
        int hackerUserId = 42;

        var playlist = new Playlist
        {
            Id = playlistId,
            Name = "Secret Playlist",
            UserId = trueOwnerId,
            CoverImagePath = "path.jpg",
            User = new User { Id = trueOwnerId, Email = "owner@test.com", PasswordHash = "x" }
        };

        _playlistRepoMock.Setup(x => x.GetPlaylistById(playlistId))
            .ReturnsAsync(playlist);

        // Act & Assert
        await _service.Invoking(s => s.Delete(playlistId, hackerUserId))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("You are not allowed to delete this playlist.");

        _playlistRepoMock.Verify(x => x.DeletePlaylist(It.IsAny<Playlist>()), Times.Never);
    }

    [Fact]
    public async Task AddToPlaylist_ShouldThrowConflictException_WhenSongAlreadyExists()
    {
        // Arrange
        int playlistId = 1;
        int songId = 5;
        int userId = 1;

        _playlistRepoMock.Setup(x => x.GetPlaylistById(playlistId))
            .ReturnsAsync(new Playlist
            {
                Id = playlistId,
                Name = "My List",
                UserId = userId,
                CoverImagePath = "img.png",
                User = new User { Id = userId, Email = "a@b.com", PasswordHash = "x" }
            });

        _playlistRepoMock.Setup(x => x.IsSongInPlaylist(playlistId, songId))
            .ReturnsAsync(true);

        // Act & Assert
        await _service.Invoking(s => s.AddToPlaylist(playlistId, songId, userId))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("Song already exists in the playlist.");
    }

    [Fact]
    public async Task GetAllByUserId_ShouldReturnMappedDtos()
    {
        // Arrange
        int userId = 1;
        var playlists = new List<Playlist>
        {
            new Playlist { Id = 1, Name = "Gym", UserId = userId, CoverImagePath = "1.jpg", User = new User { Id = userId, Email = "a@b.com", PasswordHash = "x" } },
            new Playlist { Id = 2, Name = "Study", UserId = userId, CoverImagePath = "2.jpg", User = new User { Id = userId, Email = "a@b.com", PasswordHash = "x" } }
        };

        _playlistRepoMock.Setup(x => x.GetAllPlaylistsByUserId(userId))
            .ReturnsAsync(playlists);

        _playlistRepoMock.Setup(x => x.GetLikedPlaylistIdsByUser(userId))
            .ReturnsAsync(new List<int> { 1 });

        // Act
        var result = await _service.GetAllByUserId(userId);

        // Assert
        result.Should().HaveCount(2);
        result.First(x => x.Id == 1).IsLiked.Should().BeTrue();
        result.First(x => x.Id == 2).IsLiked.Should().BeFalse();
    }

    [Fact]
    public async Task GetById_ShouldThrowNotFoundException_WhenPlaylistDoesNotExist()
    {
        // Arrange
        _playlistRepoMock.Setup(x => x.GetPlaylistById(It.IsAny<int>()))
            .ReturnsAsync((Playlist)null);

        // Act & Assert
        await _service.Invoking(s => s.GetById(999, 1))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Playlist not found.");
    }

    [Fact]
    public async Task Add_ShouldThrowValidationException_WhenDtoIsNull()
    {
        // Act & Assert
        await _service.Invoking(s => s.Add(null!, 1))
            .Should().ThrowAsync<ValidationException>();
    }
}