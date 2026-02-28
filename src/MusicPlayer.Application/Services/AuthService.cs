using MusicPlayer.Application.Dtos.Response;
using MusicPlayer.Application.Helpers;
using MusicPlayer.Domain.Exceptions;
using MusicPlayer.Domain.Interfaces;

namespace MusicPlayer.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenService _tokenService;

    public AuthService(IUserRepository userRepository, JwtTokenService tokenService)
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
