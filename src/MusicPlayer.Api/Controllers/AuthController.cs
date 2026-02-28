using Microsoft.AspNetCore.Mvc;
using MusicPlayer.Application.Dtos.Request;
using MusicPlayer.Application.Dtos.Response;
using MusicPlayer.Application.Services;

namespace MusicPlayer.Api.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenRespDto>> Login([FromForm] LoginReqDto loginReqDto)
    {
        var result = await _authService.Login(loginReqDto.Email, loginReqDto.Password);
        return result;
    }
}
