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

        builder.Property(a => a.GivenAnswer)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.CorrectAnswer)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(a => a.GameType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.IsSuccess)
            .IsRequired();

        builder.Property(a => a.AttemptNumber)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Indexes for efficient querying
        builder.HasIndex(a => a.StudentId);
        builder.HasIndex(a => new { a.StudentId, a.IsSuccess });
        builder.HasIndex(a => new { a.StudentId, a.GameType, a.Difficulty });
    }
}