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

public class SongServiceTests
{
    private readonly Mock<ISongRepository> _songRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Settings _settings;
    private readonly SongService _service;

    public SongServiceTests()
    {
        _songRepoMock = new Mock<ISongRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        _settings = new Settings
        {
            SongFolder = "TestSongs",
            ImageFolder = "TestImages",
            AllowedAudioExtensions = new[] { ".mp3", ".wav" },
            AllowedImageExtensions = new[] { ".jpg", ".png" }
        };

        _service = new SongService(_settings, _songRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task Upload_ShouldCallAddSong_WhenDataIsValid()
    {
        // Arrange
        var userId = 1;
        var dto = new SongReqDto
        {
            Title = "Stairway to Heaven",
            Artist = "Led Zeppelin",
            Duration = TimeSpan.FromMinutes(8),
            AudioFileName = "song.mp3",
            AudioStream = new MemoryStream(new byte[] { 1, 2, 3 })
        };

        _userRepoMock.Setup(x => x.GetUserById(userId))
            .ReturnsAsync(new User { Id = userId, Email = "a@b.com", PasswordHash = "x" });

        _songRepoMock.Setup(x => x.GetExsistingSong(dto.Title, dto.Artist))
            .ReturnsAsync((Song)null);

        // Act
        await _service.Upload(dto, userId);

        // Assert
        _songRepoMock.Verify(x => x.AddSong(It.Is<Song>(s => s.Title == dto.Title)), Times.Once);
    }

    [Fact]
    public async Task Stream_ShouldThrowNotFound_WhenSongDoesNotExist()
    {
        // Arrange
        _songRepoMock.Setup(x => x.GetSongById(It.IsAny<int>()))
            .ReturnsAsync((Song)null);

        // Act & Assert
        await _service.Invoking(s => s.Stream(99))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Song not found.");
    }

    [Fact]
    public async Task Delete_ShouldThrowUnauthorized_WhenUserIsNotOwner()
    {
        // Arrange
        var songId = 10;
        var ownerId = 1;
        var intruderId = 2;

        var song = new Song
        {
            Id = songId,
            UserId = ownerId,
            Title = "My Song",
            Artist = "Me",
            Duration = TimeSpan.FromMinutes(3),
            AudioFilePath = "test.mp3",
            CoverImagePath = "test.jpg",
            User = new User { Id = ownerId, Email = "x@y.dk", PasswordHash = "..." }
        };

        _songRepoMock.Setup(x => x.GetSongById(songId)).ReturnsAsync(song);

        // Act & Assert
        await _service.Invoking(s => s.Delete(songId, intruderId))
            .Should().ThrowAsync<UnauthorizedException>();

        _songRepoMock.Verify(x => x.DeleteSong(It.IsAny<Song>()), Times.Never);
    }

    [Fact]
    public async Task Upload_ShouldThrowBadRequest_WhenAudioExtensionIsInvalid()
    {
        // Arrange
        var userId = 1;
        var dto = new SongReqDto
        {
            Title = "Fake Song",
            Artist = "Hacker",
            Duration = TimeSpan.FromMinutes(3),
            AudioFileName = "virus.exe",
            AudioStream = new MemoryStream()
        };

        _userRepoMock.Setup(x => x.GetUserById(userId))
            .ReturnsAsync(new User { Id = userId, Email = "test@test.dk", PasswordHash = "hash" });

        // Act & Assert
        await _service.Invoking(s => s.Upload(dto, userId))
            .Should().ThrowAsync<BadRequestException>()
            .WithMessage("Invalid or missing audio file.");
    }

    [Fact]
    public async Task Stream_ShouldThrowNotFound_WhenFileIsMissingOnDisk()
    {
        // Arrange
        var song = new Song
        {
            Id = 1,
            AudioFilePath = "non_existent_file.mp3",
            Title = "Ghost",
            Artist = "Phantom",
            Duration = TimeSpan.FromMinutes(3),
            CoverImagePath = "img.jpg",
            User = new User { Email = "a@b.dk", PasswordHash = "x" }
        };
        _songRepoMock.Setup(x => x.GetSongById(1)).ReturnsAsync(song);

        // Act & Assert
        await _service.Invoking(s => s.Stream(1))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Song file not found.");
    }

    [Fact]
    public async Task Update_ShouldThrowException_WhenNewTitleAndArtistAlreadyExists()
    {
        // Arrange
        var songId = 1;
        var userId = 1;
        var existingSong = new Song
        {
            Id = songId,
            Title = "Old Title",
            Artist = "Artist",
            Duration = TimeSpan.Zero,
            AudioFilePath = "a.mp3",
            CoverImagePath = "c.jpg",
            UserId = userId,
            User = new User { Id = userId, Email = "x@y.dk", PasswordHash = "x" }
        };

        var otherSong = new Song
        {
            Id = 2,
            Title = "New Title",
            Artist = "Artist",
            Duration = TimeSpan.Zero,
            AudioFilePath = "b.mp3",
            CoverImagePath = "d.jpg",
            UserId = userId,
            User = new User { Id = userId, Email = "x@y.dk", PasswordHash = "x" }
        };

        _songRepoMock.Setup(x => x.GetSongById(songId)).ReturnsAsync(existingSong);
        _songRepoMock.Setup(x => x.GetExsistingSong("New Title", "Artist")).ReturnsAsync(otherSong);

        var dto = new SongReqDto { Title = "New Title", Artist = "Artist", Duration = TimeSpan.FromMinutes(4) };

        // Act & Assert
        await _service.Invoking(s => s.Update(songId, dto, userId))
            .Should().ThrowAsync<NotFoundException>();
    }
}