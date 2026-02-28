namespace MusicPlayer.Application.Dtos.Request;

public class LoginReqDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
