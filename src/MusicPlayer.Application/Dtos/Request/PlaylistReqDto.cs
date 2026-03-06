namespace MusicPlayer.Application.Dtos.Request;

public class PlaylistReqDto
{
    public required string Name { get; set; }
    public Stream? CoverImageStream { get; set; }
    public string? FileName { get; set; }
}

