using MusicPlayer.Domain.Entities;

namespace MusicPlayer.Domain.Interfaces;

public interface ISearchRepository
{
    Task<List<Playlist>> GetPlaylistsBySearch(string search);
    Task<List<Song>> GetSongsBySearch(string search);
}
