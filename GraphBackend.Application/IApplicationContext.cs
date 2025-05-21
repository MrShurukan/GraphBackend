using GraphBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphBackend.Application;

public interface IApplicationContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<HeroRecord> HeroRecords { get; set; }
    
    public Task<int> SaveChangesAsync(CancellationToken token = default);
}