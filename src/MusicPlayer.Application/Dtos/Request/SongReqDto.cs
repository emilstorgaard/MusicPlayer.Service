namespace MusicPlayer.Application.Dtos.Request;

public class SongReqDto
{
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public required TimeSpan Duration { get; set; }
    public Stream? AudioStream { get; set; }
    public string? AudioFileName { get; set; }

    public Stream? CoverImageStream { get; set; }
    public string? CoverImageFileName { get; set; }
}
