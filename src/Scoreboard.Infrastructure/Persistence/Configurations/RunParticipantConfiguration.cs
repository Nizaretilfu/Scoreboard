using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scoreboard.Domain.RunParticipants;

namespace Scoreboard.Infrastructure.Persistence.Configurations;

public sealed class RunParticipantConfiguration : IEntityTypeConfiguration<RunParticipant>
{
    public void Configure(EntityTypeBuilder<RunParticipant> builder)
    {
        builder.ToTable("run_participants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RunId).HasColumnName("run_id").IsRequired();
        builder.Property(x => x.ParticipantId).HasColumnName("participant_id").IsRequired();

        builder.HasIndex(x => new { x.RunId, x.ParticipantId }).IsUnique();

        builder.HasOne<Scoreboard.Domain.Runs.Run>()
            .WithMany()
            .HasForeignKey(x => x.RunId);

        builder.HasOne<Scoreboard.Domain.Participants.Participant>()
            .WithMany()
            .HasForeignKey(x => x.ParticipantId);
    }
}
