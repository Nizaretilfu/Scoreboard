using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scoreboard.Domain.Runs;

namespace Scoreboard.Infrastructure.Persistence.Configurations;

public sealed class RunConfiguration : IEntityTypeConfiguration<Run>
{
    public void Configure(EntityTypeBuilder<Run> builder)
    {
        builder.ToTable("runs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.HeatId).HasColumnName("heat_id").IsRequired();
        builder.Property(x => x.SequenceNumber).HasColumnName("sequence_number").IsRequired();

        builder.HasIndex(x => new { x.HeatId, x.SequenceNumber }).IsUnique();

        builder.HasOne<Scoreboard.Domain.Heats.Heat>()
            .WithMany()
            .HasForeignKey(x => x.HeatId);
    }
}
