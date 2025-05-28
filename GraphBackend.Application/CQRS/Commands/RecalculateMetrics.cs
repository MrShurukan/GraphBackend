using GraphBackend.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Commands;

public record RecalculateMetricsCommand : IRequest;

public class RecalculateMetricsCommandHandler(
    IApplicationContext context) : IRequestHandler<RecalculateMetricsCommand>
{
    public async Task Handle(RecalculateMetricsCommand request, CancellationToken token)
    {
        await context.HeroRecords
            .Where(x => x.Subscribers != 0)
            .ExecuteUpdateAsync(x => x
                    .SetProperty(z => z.ER, z => ((z.Likes + z.Comments + z.Reposts) / (float)z.Subscribers) * 100)
                    .SetProperty(z => z.VR, z => (z.Views / (float)z.Subscribers) * 100), token);
    }
}