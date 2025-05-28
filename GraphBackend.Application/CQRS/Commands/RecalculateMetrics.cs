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
        await context.Database.ExecuteSqlRawAsync(
            """
            UPDATE hero_records
            SET
                er = ((likes + comments + reposts) * 100.0) / NULLIF(CAST(subscribers AS FLOAT), 0),
                vr = (views * 100.0) / NULLIF(CAST(subscribers AS FLOAT), 0)
            WHERE subscribers != 0
            """, token);
    }
}