using GraphBackend.Application.CQRS.Commands;
using GraphBackend.Application.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GraphBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class HeroRecordsController(IMediator mediator) : ControllerBase
{
    [HttpPost("UploadCsv")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostLogin(IFormFile file, CancellationToken token)
    {
        if (file.Length == 0)
            return BadRequest("Файл пуст");

        await using var stream = file.OpenReadStream();
        var count = await mediator.Send(new UploadCsvCommand(stream), token);

        return Ok(new { Imported = count });
    }
    
    [HttpPatch("RecordsByFilter")]
    [Authorize]
    public async Task<IActionResult> PatchRecordsByFilter(GetRecordsByFilterQuery query, CancellationToken token)
    {
        var result = await mediator.Send(query, token);
        return Ok(result);
    }
}