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
        var counts = new Dictionary<HeroRecordClassification, int>();
        foreach (var heroRecordClassification in Enum.GetValues<HeroRecordClassification>().Where(x => x != HeroRecordClassification.NoHero))
        {
            counts[heroRecordClassification] =
                await finalQuery.CountAsync(x => x.Classification == heroRecordClassification, token);
        }

        return counts;
    }
}