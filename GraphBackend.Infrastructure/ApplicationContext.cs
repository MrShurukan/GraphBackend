using GraphBackend.Application;
using GraphBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Infrastructure;

public class ApplicationContext(
    DbContextOptions<ApplicationContext> options) 
    : DbContext(options), IApplicationContext
{
    public DbSet<User> Users { get; set; }
}