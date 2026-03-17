using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scoreboard.Domain.Scoring;

namespace Scoreboard.Infrastructure.Persistence.Configurations;

public sealed class ScoreEntryConfiguration : IEntityTypeConfiguration<ScoreEntry>
{
    public void Configure(EntityTypeBuilder<ScoreEntry> builder)
    {
        builder.ToTable("score_entries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RunId).HasColumnName("run_id").IsRequired();
        builder.Property(x => x.ParticipantId).HasColumnName("participant_id").IsRequired();
        builder.Property(x => x.Rings).HasColumnName("rings").IsRequired();
        builder.Property(x => x.RegisteredAtUtc).HasColumnName("registered_at_utc").IsRequired();

        builder.HasIndex(x => new { x.RunId, x.ParticipantId }).IsUnique();

        builder.HasOne<Scoreboard.Domain.Runs.Run>()
            .WithMany()
            .HasForeignKey(x => x.RunId);

        builder.HasOne<Scoreboard.Domain.Participants.Participant>()
            .WithMany()
            .HasForeignKey(x => x.ParticipantId);

        builder.ToTable(tableBuilder => tableBuilder.HasCheckConstraint("ck_score_entries_rings", "rings >= 0 AND rings <= 2"));
    }
}
