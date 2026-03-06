using FluentAssertions;
using Moq;
using MusicPlayer.Application.Services;
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
        Func<Task> act = async () => await _sut.SearchAsync(invalidQuery, 1);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Search query cannot be empty*");
    }
}
