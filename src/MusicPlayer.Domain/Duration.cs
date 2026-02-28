namespace MusicPlayer.Domain;

public record Duration // Vi bruger 'record' i C# for nem sammenligning og immutability
{
    public int Seconds { get; init; }

    public Duration(int seconds)
    {
        if (seconds < 0)
            throw new ArgumentException("En sang kan ikke have negativ varighed!");

        Seconds = seconds;
    }

    // En hjælpe-metode til at vise det pænt
    public string ToFormattedString() => $"{Seconds / 60}:{Seconds % 60:D2}";
}