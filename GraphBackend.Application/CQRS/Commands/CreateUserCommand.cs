using GraphBackend.Domain.Exceptions;
using GraphBackend.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application.CQRS.Commands;

public record CreateUserCommand(string Email, string Password, Roles Role) : IRequest<int>;

public class CreateUserCommandHandler(
    IApplicationContext context) 
    : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand request, CancellationToken token)
    {
        var email = request.Email.Trim();
        if (await context.Users.AnyAsync(x => x.Email == email, token))
        {
            throw new BadRequest400Exception("Пользователь с таким логином уже существует");
        }

        var password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User
        {
            Email = email,
            PasswordHash = password,
            Role = request.Role
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(token);

        return user.Id;
    }
}