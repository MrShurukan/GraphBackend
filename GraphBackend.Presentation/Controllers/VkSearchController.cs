using System.Text.Json.Serialization;
using GraphBackend.Application.Services;
using GraphBackend.Domain.Models;
using GraphBackend.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RestSharp;

namespace GraphBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class VkSearchController : ControllerBase
{
    private readonly IVkClient _vkClient;

    public VkSearchController(IVkClient vkClient)
    {
        _vkClient = vkClient;
    }

    [HttpGet("TestSearch")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<List<HeroRecord>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        string query,
        DateTime? startDate,
        DateTime? endDate,
        int count = 10)
    {
        if (startDate is null || endDate is null)
            return BadRequest("startDate и endDate обязательны");

        var posts = await _vkClient.SearchPostsAsync(
            query,
            new DateTimeOffset(startDate.Value, TimeSpan.Zero),
            new DateTimeOffset(endDate.Value, TimeSpan.Zero),
            count,
            HttpContext.RequestAborted);

        return Ok(posts);
    }
}