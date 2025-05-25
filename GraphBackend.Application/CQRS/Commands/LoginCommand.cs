using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Commands;

public record LoginCommand(string Email, string Password) : IRequest<string>;

public class LoginCommandHandler(
    IApplicationContext context,
    IJwtTokenGenerator tokenGenerator) 
    : IRequestHandler<LoginCommand, string>
{
    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Неверный логин или пароль");
        }

        return tokenGenerator.GenerateToken(user.Id, user.Email, user.Role);
    }
}