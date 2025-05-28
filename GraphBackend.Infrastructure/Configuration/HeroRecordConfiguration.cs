using GraphBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GraphBackend.Infrastructure.Configuration;

public class HeroRecordConfiguration : IEntityTypeConfiguration<HeroRecord>
{
    public void Configure(EntityTypeBuilder<HeroRecord> builder)
    {
        builder.HasIndex(x => x.Url).IsUnique();
        builder.HasIndex(x => x.Subscribers);
    }
}