using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayer.Application.Dtos.Request;
using MusicPlayer.Application.Helpers;
using MusicPlayer.Application.Interfaces;

namespace MusicPlayer.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _userService.GetAll();
        return Ok(result);
    }

    [Authorize]
    [HttpGet("authorized")]
    public async Task<IActionResult> Get()
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _userService.GetUser(userId);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> AddUser([FromForm] UserReqDto userReqDto)
    {
        await _userService.AddUser(userReqDto);
        return Ok("User registered successfully.");
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        int userId = UserHelper.GetUserId(User);

        await _userService.Delete(userId);
        return Ok("User was successfully deleted");
    }
}
