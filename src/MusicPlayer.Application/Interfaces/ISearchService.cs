using MusicPlayer.Application.Dtos.Response;

namespace MusicPlayer.Application.Interfaces;

public interface ISearchService
{
    Task<SearchRespDto> SearchAsync(string q, int userId);
}