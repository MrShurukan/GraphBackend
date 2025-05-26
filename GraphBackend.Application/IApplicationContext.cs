using GraphBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GraphBackend.Application;

public interface IApplicationContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<HeroRecord> HeroRecords { get; set; }
    
    public Task<int> SaveChangesAsync(CancellationToken token = default);
    public DatabaseFacade Database { get; }
    public ChangeTracker ChangeTracker { get; }
}