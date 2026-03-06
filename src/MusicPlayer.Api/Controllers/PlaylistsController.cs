using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayer.Api.Mappers;
using MusicPlayer.Api.Models;
using MusicPlayer.Application.Dtos.Response;
using MusicPlayer.Application.Helpers;
using MusicPlayer.Application.Interfaces;

namespace MusicPlayer.Api.Controllers;

[Route("api/playlists")]
[ApiController]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;

    public PlaylistsController(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<PlaylistRespDto>>> GetAllByUserId()
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _playlistService.GetAllByUserId(userId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlaylistRespDto>> GetById(int id)
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _playlistService.GetById(id, userId);
        return Ok(result);
    }

    [HttpGet("{id:int}/cover")]
    public async Task<IActionResult> GetCoverImageById(int id)
    {
        var coverFilePath = await _playlistService.GetCoverPathByPlaylistId(id);

        if (string.IsNullOrEmpty(coverFilePath) || !System.IO.File.Exists(coverFilePath))
        {
            return NotFound("Cover image not found for this playlist.");
        }

        return PhysicalFile(coverFilePath, "image/jpeg");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add([FromForm] PlaylistReqDtoWeb webDto)
    {
        int userId = UserHelper.GetUserId(User);

        var applicationDto = PlaylistWebMapper.MapToApplicationDto(webDto);

        await _playlistService.Add(applicationDto, userId);
        return StatusCode(201, "Playlist was successfully added");
    }

    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<IActionResult> Like(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Like(id, userId);
        return Ok("Playlist was liked successfully");
    }

    [Authorize]
    [HttpPost("{id:int}/dislike")]
    public async Task<IActionResult> Dislike(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Dislike(id, userId);
        return Ok("Playlist was successfully disliked");
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] PlaylistReqDtoWeb webDto)
    {
        int userId = UserHelper.GetUserId(User);

        var applicationDto = PlaylistWebMapper.MapToApplicationDto(webDto);

        await _playlistService.Update(id, applicationDto, userId);
        return Ok("Playlist was successfully updated");
    }

    [Authorize]
    [HttpPut("{id:int}/cover/remove")]
    public async Task<IActionResult> RemoveCoverImageById(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.UpdateCoverImage(id, userId);
        return Ok("Playlist cover image was successfully removed");
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.Delete(id, userId);
        return Ok("Playlist was successfully deleted");
    }

    [Authorize]
    [HttpPost("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> AddSongToPlaylist(int playlistId, int songId)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.AddToPlaylist(playlistId, songId, userId);
        return Ok("Song was successfully added to playlist");
    }

    [HttpGet("{id}/songs")]
    public async Task<ActionResult<List<SongRespDto>>> GetAllSongsByPlaylistId(int id)
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _playlistService.GetAllSongsByPlaylistId(id, userId);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{playlistId}/songs/{songId}")]
    public async Task<IActionResult> RemoveSongFromPlaylist(int playlistId, int songId)
    {
        int userId = UserHelper.GetUserId(User);

        await _playlistService.RemoveFromPlaylist(playlistId, songId, userId);
        return Ok("Song was successfully removed from playlist");
    }
}