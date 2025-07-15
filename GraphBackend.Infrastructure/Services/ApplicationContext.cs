using GraphBackend.Application;
using GraphBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Infrastructure.Services;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) 
    : DbContext(options), IApplicationContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<HeroRecord> HeroRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(assembly: typeof(ApplicationContext).Assembly);
    }
}