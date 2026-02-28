using Microsoft.AspNetCore.Http;

namespace MusicPlayer.Application.Dtos.Request;

public class PlaylistReqDto
{
    public required string Name { get; set; }
    public IFormFile? CoverImageFile { get; set; }
}

