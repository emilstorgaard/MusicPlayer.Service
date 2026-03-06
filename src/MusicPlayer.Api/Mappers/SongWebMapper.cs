using MusicPlayer.Api.Models;
using MusicPlayer.Application.Dtos.Request;

namespace MusicPlayer.Api.Mappers;

public class SongWebMapper
{
    public static SongReqDto MapToApplicationDto(SongReqDtoWeb webDto)
    {
        return new SongReqDto
        {
            Title = webDto.Title,
            Artist = webDto.Artist,
            Duration = webDto.Duration,
            AudioStream = webDto.AudioFile?.OpenReadStream(),
            AudioFileName = webDto.AudioFile?.FileName,
            CoverImageStream = webDto.CoverImageFile?.OpenReadStream(),
            CoverImageFileName = webDto.CoverImageFile?.FileName
        };
    }
}
