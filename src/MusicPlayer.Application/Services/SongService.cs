using MusicPlayer.Application.Configurations;
using MusicPlayer.Application.Dtos.Request;
using MusicPlayer.Application.Helpers;
using MusicPlayer.Application.Interfaces;
using MusicPlayer.Domain.Entities;
using MusicPlayer.Domain.Exceptions;
using MusicPlayer.Domain.Interfaces;

namespace MusicPlayer.Application.Services;

public class SongService : ISongService
{
    private readonly Settings _settings;
    private readonly ISongRepository _songRepository;
    private readonly IUserRepository _userRepository;

    public SongService(Settings settings, ISongRepository songRepository, IUserRepository userRepository)
    {
        _settings = settings;
        Directory.CreateDirectory(_settings.SongFolder);
        Directory.CreateDirectory(_settings.ImageFolder);
        _songRepository = songRepository;
        _userRepository = userRepository;
    }

    public async Task<FileStream> Stream(int id)
    {
        var song = await _songRepository.GetSongById(id);
        if (song == null) throw new NotFoundException("Song not found.");

        if (string.IsNullOrEmpty(song?.AudioFilePath)) throw new NotFoundException("Song file path is missing.");
        var audioFilePath = FileHelper.GetFullPath(Path.Combine(_settings.SongFolder, song.AudioFilePath));

        if (!File.Exists(audioFilePath)) throw new NotFoundException("Song file not found.");

        var fileStream = File.OpenRead(audioFilePath);
        return fileStream;
    }

    public async Task<string> GetCoverPathBySongId(int songId)
    {
        var song = await _songRepository.GetSongById(songId);

        if (song == null || string.IsNullOrEmpty(song.CoverImagePath))
            return null;

        return FileHelper.GetFullPath(Path.Combine(_settings.ImageFolder, song.CoverImagePath));
    }

    public async Task Upload(SongReqDto songDto, int userId)
    {
        if (songDto == null) throw new BadRequestException("Invalid song data.");

        var user = await _userRepository.GetUserById(userId);
        if (user == null) throw new NotFoundException("User not found.");

        var existingSong = await _songRepository.GetExsistingSong(songDto.Title, songDto.Artist);
        if (existingSong != null) throw new ConflictException("A song with the same title and artist already exists.");

        if (songDto.AudioStream == null || !FileHelper.IsValidExtension(songDto.AudioFileName, _settings.AllowedAudioExtensions))
            throw new BadRequestException("Invalid or missing audio file.");

        var audioFilePath = await FileHelper.SaveFile(songDto.AudioStream, songDto.AudioFileName!, _settings.SongFolder);

        string coverImagePath;
        if (songDto.CoverImageStream != null && FileHelper.IsValidExtension(songDto.CoverImageFileName, _settings.AllowedImageExtensions))
        {
            coverImagePath = await FileHelper.SaveFile(
                songDto.CoverImageStream,
                songDto.CoverImageFileName!,
                _settings.ImageFolder);
        }
        else
        {
            coverImagePath = FileHelper.GetDefaultCoverImagePath(_settings.ImageFolder);
        }


        var song = new Song
        {
            Title = songDto.Title,
            Artist = songDto.Artist,
            Duration = songDto.Duration,
            AudioFilePath = audioFilePath,
            CoverImagePath = coverImagePath,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            User = user
        };

        await _songRepository.AddSong(song);
    }

    public async Task Like(int songId, int userId)
    {
        var isAlreadyLiked = await _songRepository.IsSongLikedByUser(songId, userId);
        if (isAlreadyLiked) throw new ConflictException("Song already liked.");

        var likedSong = new LikedSong
        {
            UserId = userId,
            SongId = songId
        };

        var song = await _songRepository.GetSongById(songId);
        if (song == null) throw new NotFoundException("Song not found.");

        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.LikeSong(likedSong);
    }

    public async Task Dislike(int songId, int userId)
    {
        var likedSong = await _songRepository.GetLikedSongByUser(songId, userId);
        if (likedSong == null) throw new NotFoundException("Song not found in your liked songs.");

        var song = await _songRepository.GetSongById(songId);
        if (song == null) throw new NotFoundException("Song not found.");

        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.DislikeSong(likedSong);
    }

    public async Task UpdateCoverImage(int songId, int userId)
    {
        var song = await _songRepository.GetSongById(songId);
        if (song == null) throw new NotFoundException("Song not found.");
        if (song.UserId != userId) throw new UnauthorizedException("You are not allowed to update this song.");

        FileHelper.DeleteFile(_settings.ImageFolder, song.CoverImagePath);

        song.CoverImagePath = FileHelper.GetDefaultCoverImagePath(_settings.ImageFolder);
        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.UpdateSong(song);
    }

    public async Task Update(int id, SongReqDto songDto, int userId)
    {
        var song = await _songRepository.GetSongById(id);
        if (song == null) throw new NotFoundException("Song was not found.");
        if (song.UserId != userId) throw new UnauthorizedException("You are not allowed to update this song.");

        var existingSong = await _songRepository.GetExsistingSong(songDto.Title, songDto.Artist);
        if (existingSong != null && existingSong.Id != id) throw new NotFoundException("A song with the same title and artist already exists.");

        if (songDto.CoverImageStream != null && FileHelper.IsValidExtension(songDto.CoverImageFileName, _settings.AllowedImageExtensions))
        {
            FileHelper.DeleteFile(_settings.ImageFolder, song.CoverImagePath);
            song.CoverImagePath = await FileHelper.SaveFile(songDto.CoverImageStream, songDto.CoverImageFileName, _settings.ImageFolder);
        }

        if (songDto.AudioStream != null && FileHelper.IsValidExtension(songDto.AudioFileName, _settings.AllowedAudioExtensions))
        {
            FileHelper.DeleteFile(_settings.SongFolder, song.AudioFilePath);
            song.AudioFilePath = await FileHelper.SaveFile(songDto.AudioStream, songDto.AudioFileName, _settings.SongFolder);
        }

        song.Title = songDto.Title;
        song.Artist = songDto.Artist;
        song.Duration = songDto.Duration;
        song.UpdatedAtUtc = DateTime.UtcNow;

        await _songRepository.UpdateSong(song);
    }

    public async Task Delete(int id, int userId)
    {
        var song = await _songRepository.GetSongById(id);
        if (song == null) throw new NotFoundException("Song not found.");
        if (song.UserId != userId) throw new UnauthorizedException("You are not allowed to delete this song.");

        FileHelper.DeleteFile(_settings.SongFolder, song.AudioFilePath);
        FileHelper.DeleteFile(_settings.ImageFolder, song.CoverImagePath);

        await _songRepository.DeleteSong(song);
    }
}
