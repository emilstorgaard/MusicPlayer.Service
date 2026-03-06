using MusicPlayer.Application.Dtos.Response;

namespace MusicPlayer.Application.Interfaces;

public interface IAuthService
{
    Task<TokenRespDto> Login(string email, string password);
}