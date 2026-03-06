using MusicPlayer.Api.Models;
using MusicPlayer.Application.Dtos.Request;

namespace MusicPlayer.Api.Mappers;

public class PlaylistWebMapper
{
    public static PlaylistReqDto MapToApplicationDto(PlaylistReqDtoWeb webDto)
    {
        return new PlaylistReqDto
        {
            Name = webDto.Name,
            CoverImageStream = webDto.CoverImageFile?.OpenReadStream(),
            FileName = webDto.CoverImageFile?.FileName
        };
    }
}
