using GraphBackend.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Queries;

public record GetMetricsQuery(GetRecordsByFilterQuery FilterQuery) : IRequest<List<MetricDto>>;

public class GetGetMetricsQueryQueryHandler(
    IApplicationContext context)
    : IRequestHandler<GetMetricsQuery, List<MetricDto>>
{
    public async Task<List<MetricDto>> Handle(GetMetricsQuery request, CancellationToken token)
    {
        var finalQuery = GetRecordsByFilterQueryHandler.GetFilterFromQuery(request.FilterQuery, context.HeroRecords);
        var metrics = await finalQuery
            .GroupBy(x => DateOnly.FromDateTime(x.DateTime)) // группировка по дате
            .Select(group => new MetricDto
            {
                Date = group.Key,
                VR = group.Average(x => x.VR),
                ER = group.Average(x => x.ER),
                Average = group.Sum(x => x.Views) / (float)group.Count()
            })
            .OrderBy(m => m.Date)
            .ToListAsync(token);

        return metrics;
    }
}

public class MetricDto
{
    public float VR { get; set; }
    public float ER { get; set; }
    public float Average { get; set; }
    public DateOnly Date { get; set; }
}
