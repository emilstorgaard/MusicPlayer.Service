using MusicPlayer.Application.Dtos.Request;

namespace MusicPlayer.Application.Interfaces;

public interface ISongService
{
    Task<FileStream> Stream(int id);
    Task<string> GetCoverPathBySongId(int songId);
    Task Upload(SongReqDto songDto, int userId);
    Task Like(int songId, int userId);
    Task Dislike(int songId, int userId);
    Task UpdateCoverImage(int songId, int userId);
    Task Update(int id, SongReqDto songDto, int userId);
    Task Delete(int id, int userId);
}