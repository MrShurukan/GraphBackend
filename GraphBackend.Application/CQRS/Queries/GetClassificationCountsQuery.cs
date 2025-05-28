using GraphBackend.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Queries;

public record GetClassificationCountsQuery(GetRecordsByFilterQuery FilterQuery) : IRequest<Dictionary<HeroRecordClassification, int>>;

public class GetClassificationCountsQueryHandler(
    IApplicationContext context)
    : IRequestHandler<GetClassificationCountsQuery, Dictionary<HeroRecordClassification, int>>
{
    public async Task<Dictionary<HeroRecordClassification, int>> Handle(GetClassificationCountsQuery request, CancellationToken token)
    {
        var finalQuery = GetRecordsByFilterQueryHandler.GetFilterFromQuery(request.FilterQuery, context.HeroRecords);
        var groupedCounts = await finalQuery
            .GroupBy(x => x.Classification)
            .Select(g => new
            {
                Classification = g.Key,
                Count = g.Count()
            })
            .ToListAsync(token);
        
        var allClassifications = Enum.GetValues<HeroRecordClassification>()
            .Where(c => c != HeroRecordClassification.NoHero);

        var counts = allClassifications
            .ToDictionary(
                c => c,
                c => groupedCounts.FirstOrDefault(g => g.Classification == c)?.Count ?? 0
            );

        return counts;
    }
}