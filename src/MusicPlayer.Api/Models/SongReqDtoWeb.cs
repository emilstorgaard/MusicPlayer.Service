namespace MusicPlayer.Api.Models;

public class SongReqDtoWeb
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public TimeSpan Duration { get; set; }
    public IFormFile? AudioFile { get; set; }
    public IFormFile? CoverImageFile { get; set; }
}
