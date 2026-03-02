namespace MusicPlayer.Application.Configurations;

public class Settings
{
    public string SongFolder { get; set; }
    public string ImageFolder { get; set; }
    public string[] AllowedAudioExtensions { get; set; }
    public string[] AllowedImageExtensions { get; set; }
    public string JwtSecret { get; set; }
    public int JwtExpiryHours { get; set; }
}
