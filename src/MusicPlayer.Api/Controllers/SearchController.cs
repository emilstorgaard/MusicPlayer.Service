using Microsoft.AspNetCore.Mvc;
using MusicPlayer.Application.Dtos.Response;
using MusicPlayer.Application.Helpers;
using MusicPlayer.Application.Services;

namespace MusicPlayer.Api.Controllers;

[Route("api/search")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;

    public SearchController(SearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<SearchRespDto>> Search([FromQuery] string q)
    {
        int userId = UserHelper.GetUserId(User);

        var result = await _searchService.SearchAsync(q, userId);
        return Ok(result);
    }
}