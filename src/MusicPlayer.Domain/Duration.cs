namespace MusicPlayer.Domain;

public record Duration
{
    public int Seconds { get; init; }

    public Duration(int seconds)
    {
        if (seconds < 0)
            throw new ArgumentException("En sang kan ikke have negativ varighed!");

        Seconds = seconds;
    }

    public string ToFormattedString() => $"{Seconds / 60}:{Seconds % 60:D2}";
}