using MusicPlayer.Application.Dtos.Request;
using MusicPlayer.Application.Dtos.Response;

namespace MusicPlayer.Application.Interfaces;

public interface IUserService
{
    Task<List<UserRespDto>> GetAll();
    Task<UserRespDto> GetUser(int userId);
    Task AddUser(UserReqDto userReqDto);
    Task Delete(int userId);
}