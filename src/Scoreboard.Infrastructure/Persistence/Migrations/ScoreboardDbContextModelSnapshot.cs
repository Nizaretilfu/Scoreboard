using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Scoreboard.Infrastructure.Persistence;

#nullable disable

namespace Scoreboard.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ScoreboardDbContext))]
partial class ScoreboardDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.10")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Scoreboard.Domain.Competitions.Competition", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
                b.Property<DateOnly>("CompetitionDate").HasColumnType("date").HasColumnName("competition_date");
                b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)").HasColumnName("name");
                b.HasKey("Id");
                b.ToTable("competitions", (string)null);
            });

        modelBuilder.Entity("Scoreboard.Domain.Heats.Heat", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
                b.Property<Guid>("CompetitionId").HasColumnType("uuid").HasColumnName("competition_id");
                b.Property<int>("SequenceNumber").HasColumnType("integer").HasColumnName("sequence_number");
                b.HasKey("Id");
                b.HasIndex("CompetitionId", "SequenceNumber").IsUnique();
                b.ToTable("heats", (string)null);
            });

        modelBuilder.Entity("Scoreboard.Domain.Participants.Participant", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
                b.Property<Guid>("CompetitionId").HasColumnType("uuid").HasColumnName("competition_id");
                b.Property<string>("Name").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)").HasColumnName("name");
                b.Property<int>("Number").HasColumnType("integer").HasColumnName("number");
                b.HasKey("Id");
                b.HasIndex("CompetitionId", "Number").IsUnique();
                b.ToTable("participants", (string)null);
            });

        modelBuilder.Entity("Scoreboard.Domain.RunParticipants.RunParticipant", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
                b.Property<Guid>("ParticipantId").HasColumnType("uuid").HasColumnName("participant_id");
                b.Property<Guid>("RunId").HasColumnType("uuid").HasColumnName("run_id");
                b.HasKey("Id");
                b.HasIndex("ParticipantId");
                b.HasIndex("RunId", "ParticipantId").IsUnique();
                b.ToTable("run_participants", (string)null);
            });

        modelBuilder.Entity("Scoreboard.Domain.Runs.Run", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
                b.Property<Guid>("HeatId").HasColumnType("uuid").HasColumnName("heat_id");
                b.Property<int>("SequenceNumber").HasColumnType("integer").HasColumnName("sequence_number");
                b.HasKey("Id");
                b.HasIndex("HeatId", "SequenceNumber").IsUnique();
                b.ToTable("runs", (string)null);
            });

        modelBuilder.Entity("Scoreboard.Domain.Scoring.ScoreEntry", b =>
            {
                b.Property<Guid>("Id").HasColumnType("uuid").HasColumnName("id");
                b.Property<Guid>("ParticipantId").HasColumnType("uuid").HasColumnName("participant_id");
                b.Property<DateTimeOffset>("RegisteredAtUtc").HasColumnType("timestamp with time zone").HasColumnName("registered_at_utc");
                b.Property<int>("Rings").HasColumnType("integer").HasColumnName("rings");
                b.Property<Guid>("RunId").HasColumnType("uuid").HasColumnName("run_id");
                b.HasKey("Id");
                b.HasIndex("ParticipantId");
                b.HasIndex("RunId", "ParticipantId").IsUnique();
                b.ToTable("score_entries", (string)null);
                b.ToTable("score_entries", t => t.HasCheckConstraint("ck_score_entries_rings", "rings >= 0 AND rings <= 2"));
            });

        modelBuilder.Entity("Scoreboard.Domain.Heats.Heat", b =>
            {
                b.HasOne("Scoreboard.Domain.Competitions.Competition", null)
                    .WithMany()
                    .HasForeignKey("CompetitionId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Scoreboard.Domain.Participants.Participant", b =>
            {
                b.HasOne("Scoreboard.Domain.Competitions.Competition", null)
                    .WithMany()
                    .HasForeignKey("CompetitionId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Scoreboard.Domain.RunParticipants.RunParticipant", b =>
            {
                b.HasOne("Scoreboard.Domain.Participants.Participant", null)
                    .WithMany()
                    .HasForeignKey("ParticipantId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Scoreboard.Domain.Runs.Run", null)
                    .WithMany()
                    .HasForeignKey("RunId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Scoreboard.Domain.Runs.Run", b =>
            {
                b.HasOne("Scoreboard.Domain.Heats.Heat", null)
                    .WithMany()
                    .HasForeignKey("HeatId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

        modelBuilder.Entity("Scoreboard.Domain.Scoring.ScoreEntry", b =>
            {
                b.HasOne("Scoreboard.Domain.Participants.Participant", null)
                    .WithMany()
                    .HasForeignKey("ParticipantId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Scoreboard.Domain.Runs.Run", null)
                    .WithMany()
                    .HasForeignKey("RunId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });
#pragma warning restore 612, 618
    }
}
