using MusicPlayer.Application.Dtos.Response;
using MusicPlayer.Application.Helpers;
using MusicPlayer.Application.Interfaces;
using MusicPlayer.Domain.Exceptions;
using MusicPlayer.Domain.Interfaces;

namespace MusicPlayer.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _tokenService;

    public AuthService(IUserRepository userRepository, IJwtTokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<TokenRespDto> Login(string email, string password)
    {
        var user = await _userRepository.GetUserByEmail(email);
        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash)) throw new UnauthorizedException("Invalid email or password.");

        var token = _tokenService.GenerateToken(user);

        var tokenRespDto = new TokenRespDto
        {
            Token = token
        };

        return tokenRespDto;
    }
}
