using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GraphBackend.Application.CQRS.Commands;
using GraphBackend.Application.CQRS.Queries;
using GraphBackend.Domain.Models;
using GraphBackend.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IMediator mediator)
    : ControllerBase
{
    #if DEBUG
    [HttpPost("CreateAdmin")]
    public async Task<IActionResult> CreateAdmin([FromServices] ApplicationContext context, CancellationToken token)
    {
        if (await context.Users.AnyAsync(x => x.Role == Roles.Admin, token))
        {
            throw new Exception("Админ уже существует");
        }

        var password = BCrypt.Net.BCrypt.HashPassword("admin");
        var user = new User
        {
            Email = "test@admin.com",
            PasswordHash = password,
            Role = Roles.Admin
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(token);
        
        return Ok();
    }
    #endif
    
    [HttpPost("Login")]
    public async Task<IActionResult> PostLogin(LoginCommand command, CancellationToken token)
    {
        var result = await mediator.Send(command, token);
        return Ok(new { Token = result });
    }
    
    [HttpPost("CreateUser")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PostCreateUser(CreateUserCommand command, CancellationToken token)
    {
        var result = await mediator.Send(command, token);
        return Ok(new { id = result });
    }
    
    [HttpDelete("User")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser([FromQuery] int id, CancellationToken token)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new DeleteUserCommand(id, userId);
        await mediator.Send(command, token);
        return Ok();
    }

    [HttpPatch("UsersByFilter")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchUsersByFilter(GetUsersByFilterQuery query, CancellationToken token)
    {
        var result = await mediator.Send(query, token);
        return Ok(result);
    }

    [HttpGet("TestAuth")]
    [Authorize]
    public async Task<IActionResult> GetTestAuth([FromServices] ApplicationContext context, CancellationToken token)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await context.Users.FirstAsync(x => x.Id == userId, token);
        return Ok($"Привет, {user.Email}, с ролью {role}");
    }
}