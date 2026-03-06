namespace MusicPlayer.Api.Models;

public class PlaylistReqDtoWeb
{
    public string Name { get; set; }
    public IFormFile? CoverImageFile { get; set; }
}