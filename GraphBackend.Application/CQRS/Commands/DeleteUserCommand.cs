using GraphBackend.Application.Extensions;
using GraphBackend.Domain.Exceptions;
using MediatR;

namespace GraphBackend.Application.CQRS.Commands;

public record DeleteUserCommand(int Id, int RequesterId) : IRequest;

public class DeleteUserCommandHandler(
    IApplicationContext context)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken token)
    {
        if (request.Id == request.RequesterId)
            throw new Forbidden403Exception("Невозможно удалить самого себя");
        
        var user = await context.Users.FindRequiredAsyncIntId(request.Id, token);
        context.Users.Remove(user);
        await context.SaveChangesAsync(token);
    }
}