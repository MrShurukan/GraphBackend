using GraphBackend.Domain.Models;

namespace GraphBackend.Application;

public interface IJwtTokenGenerator
{
    string GenerateToken(int userId, string email, Roles role);
}