using MusicPlayer.Application.Dtos.Request;
using MusicPlayer.Application.Dtos.Response;

namespace MusicPlayer.Application.Interfaces;

public interface IPlaylistService
{
    Task<List<PlaylistRespDto>> GetAllByUserId(int userId);
    Task<PlaylistRespDto> GetById(int id, int userId);
    Task<string> GetCoverPathByPlaylistId(int playlistId);
    Task Add(PlaylistReqDto playlistReqDto, int userId);
    Task Like(int playlistId, int userId);
    Task Dislike(int playlistId, int userId);
    Task UpdateCoverImage(int playlistId, int userId);
    Task Update(int id, PlaylistReqDto playlistDto, int userId);
    Task Delete(int id, int userId);
    Task AddToPlaylist(int playlistId, int songId, int userId);
    Task<List<SongRespDto>> GetAllSongsByPlaylistId(int playlistId, int userId);
    Task RemoveFromPlaylist(int playlistId, int songId, int userId);
}