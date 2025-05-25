using GraphBackend.Domain.Common;

namespace GraphBackend.Domain.Models;

public class User : BaseEntity
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Roles Role { get; set; } = Roles.User;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public Roles Role { get; set; } = Roles.User;
}