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
    [RequestSizeLimit(1_000_000_000_000)]
    public async Task<IActionResult> PostLogin(IFormFile file, CancellationToken token)
    {
        if (file.Length == 0)
            return BadRequest("Файл пуст");

        await using var stream = file.OpenReadStream();
        var count = await mediator.Send(new UploadCsvCommand(stream), token);

        return Ok(new { Imported = count });
    }
    
    [HttpPost("Mark")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostMarkRecords(CancellationToken token)
    {
        var result = await mediator.Send(new MarkHeroRecordsCommand(), token);
        return Ok(result);
    }
    
    [HttpPost("ResetMark")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostResetMarkRecords(CancellationToken token)
    {
        await mediator.Send(new ResetMarkRecordsCommand(), token);
        return Ok();
    }
    
    [HttpPost("RecalculateMetrics")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostRecalculateMetrics(CancellationToken token)
    {
        await mediator.Send(new RecalculateMetricsCommand(), token);
        return Ok();
    }
    
    [HttpPatch("ClassificationCounts")]
    [Authorize]
    public async Task<IActionResult> GetClassificationCounts([FromBody] GetRecordsByFilterQuery query, CancellationToken token)
    {
        var result = await mediator.Send(new GetClassificationCountsQuery(query), token);
        return Ok(result);
    }
    
    [HttpPatch("GetMetrics")]
    [Authorize]
    public async Task<IActionResult> GetMetrics([FromBody] GetRecordsByFilterQuery query, CancellationToken token)
    {
        var result = await mediator.Send(new GetMetricsQuery(query), token);
        return Ok(result);
    }
    
    [HttpPatch("RecordsByFilter")]
    [Authorize]
    public async Task<IActionResult> PatchRecordsByFilter(GetRecordsByFilterQuery query, CancellationToken token)
    {
        var result = await mediator.Send(query, token);
        return Ok(result);
    }
}