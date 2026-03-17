using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scoreboard.Domain.Heats;

namespace Scoreboard.Infrastructure.Persistence.Configurations;

public sealed class HeatConfiguration : IEntityTypeConfiguration<Heat>
{
    public void Configure(EntityTypeBuilder<Heat> builder)
    {
        builder.ToTable("heats");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CompetitionId).HasColumnName("competition_id").IsRequired();
        builder.Property(x => x.SequenceNumber).HasColumnName("sequence_number").IsRequired();

        builder.HasIndex(x => new { x.CompetitionId, x.SequenceNumber }).IsUnique();

        builder.HasOne<Scoreboard.Domain.Competitions.Competition>()
            .WithMany()
            .HasForeignKey(x => x.CompetitionId);
    }
}
