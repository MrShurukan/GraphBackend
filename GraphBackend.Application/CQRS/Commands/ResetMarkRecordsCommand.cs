using GraphBackend.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Commands;

public record ResetMarkRecordsCommand : IRequest;

public class ResetMarkRecordsCommandHandler(
    IApplicationContext context) : IRequestHandler<ResetMarkRecordsCommand>
{
    public async Task Handle(ResetMarkRecordsCommand request, CancellationToken token)
    {
        await context.HeroRecords
            .ExecuteUpdateAsync(x => x.SetProperty(z => z.Classification,
                (HeroRecordClassification)0), cancellationToken: token);
    }
}