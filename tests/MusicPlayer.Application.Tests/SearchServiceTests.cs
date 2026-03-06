using FluentAssertions;
using Moq;
using MusicPlayer.Application.Services;
using MusicPlayer.Domain.Entities;
using MusicPlayer.Domain.Exceptions;
using MusicPlayer.Domain.Interfaces;
using Xunit;

namespace MusicPlayer.Application.Tests;

public class SearchServiceTests
{
    private readonly Mock<ISearchRepository> _searchRepoMock = new();
    private readonly Mock<IPlaylistRepository> _playlistRepoMock = new();
    private readonly Mock<ISongRepository> _songRepoMock = new();
    private readonly SearchService _sut;
    public SearchServiceTests()
    {
        _sut = new SearchService(
            _searchRepoMock.Object,
            _playlistRepoMock.Object,
            _songRepoMock.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task SearchAsync_ShouldThrowBadRequestException_WhenQueryIsInvalid(string invalidQuery)
    {
        // Arrange
        var userId = 1;

        // Act
        Func<Task> act = async () => await _sut.SearchAsync(invalidQuery, userId);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Search query cannot be empty*");
    }

    [Fact]
    public async Task SearchAsync_ShouldLowercaseQuery_BeforeCallingRepositories()
    {
        // Arrange
        var upperQuery = "METALLICA";
        var expectedLower = "metallica";
        var userId = 1;

        _searchRepoMock.Setup(x => x.GetSongsBySearch(It.IsAny<string>()))
            .ReturnsAsync(new List<Song>());
        _searchRepoMock.Setup(x => x.GetPlaylistsBySearch(It.IsAny<string>()))
            .ReturnsAsync(new List<Playlist>());
        _playlistRepoMock.Setup(x => x.GetLikedPlaylistIdsByUser(userId))
            .ReturnsAsync(new List<int>());
        _songRepoMock.Setup(x => x.GetLikedSongIdsByUser(userId))
            .ReturnsAsync(new List<int>());

        // Act
        await _sut.SearchAsync(upperQuery, userId);

        // Assert
        _searchRepoMock.Verify(x => x.GetSongsBySearch(expectedLower), Times.Once);
        _searchRepoMock.Verify(x => x.GetPlaylistsBySearch(expectedLower), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ShouldPropagateException_WhenRepositoryFails()
    {
        // Arrange
        var query = "Queen";
        var userId = 1;

        _searchRepoMock.Setup(x => x.GetPlaylistsBySearch(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        Func<Task> act = async () => await _sut.SearchAsync(query, userId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task SearchAsync_ShouldCorrectlyMapSongsAndLikedStatus()
    {
        // Arrange
        var userId = 1;
        var query = "test";

        var testUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hashed_password"
        };

        var song = new Song
        {
            Id = 10,
            Title = "Test Song",
            Artist = "Test Artist",
            Duration = TimeSpan.FromMinutes(3),
            AudioFilePath = "path/to/audio",
            CoverImagePath = "path/to/cover",
            User = testUser,
            UserId = userId
        };

        _searchRepoMock.Setup(x => x.GetSongsBySearch(query))
            .ReturnsAsync(new List<Song> { song });

        _songRepoMock.Setup(x => x.GetLikedSongIdsByUser(userId))
            .ReturnsAsync(new List<int> { 10 });

        _searchRepoMock.Setup(x => x.GetPlaylistsBySearch(query))
            .ReturnsAsync(new List<Playlist>());

        _playlistRepoMock.Setup(x => x.GetLikedPlaylistIdsByUser(userId))
            .ReturnsAsync(new List<int>());

        // Act
        var result = await _sut.SearchAsync(query, userId);

        // Assert
        result.Songs.Should().HaveCount(1);
        result.Songs[0].Title.Should().Be("Test Song");
        result.Songs[0].IsLiked.Should().BeTrue();
    }
}
