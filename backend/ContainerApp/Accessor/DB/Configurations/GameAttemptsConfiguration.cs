using Accessor.Models.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class GameAttemptsConfiguration : IEntityTypeConfiguration<GameAttempt>
{
    public void Configure(EntityTypeBuilder<GameAttempt> builder)
    {
        builder.ToTable("GameAttempts");

        builder.HasKey(a => a.AttemptId);

        builder.Property(a => a.ExerciseId)
            .IsRequired();

        builder.Property(a => a.GivenAnswer)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.CorrectAnswer)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.GameType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.AttemptNumber)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes for efficient querying
        builder.HasIndex(a => a.StudentId);
        builder.HasIndex(a => a.ExerciseId);
        builder.HasIndex(a => new { a.StudentId, a.Status });
        builder.HasIndex(a => new { a.StudentId, a.GameType, a.Difficulty });
        builder.HasIndex(a => new { a.ExerciseId, a.AttemptNumber });
    }
}