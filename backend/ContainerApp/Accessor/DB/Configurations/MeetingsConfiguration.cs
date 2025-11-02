using Accessor.Models.Meetings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class MeetingsConfiguration : IEntityTypeConfiguration<MeetingModel>
{
    public void Configure(EntityTypeBuilder<MeetingModel> builder)
    {
        builder.ToTable("Meetings");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Attendees)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(m => m.StartTimeUtc)
            .IsRequired();

        builder.Property(m => m.DurationMinutes)
            .IsRequired();

        builder.Property(m => m.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.GroupCallId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.CreatedOn)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(m => m.CreatedByUserId)
            .IsRequired();

        // Check constraint for DurationMinutes (1-1440 minutes = 1 min to 24 hours)
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Meetings_DurationMinutes",
            "\"DurationMinutes\" >= 1 AND \"DurationMinutes\" <= 1440"));

        // Indexes for efficient querying
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.StartTimeUtc);
        builder.HasIndex(m => m.CreatedByUserId);
        builder.HasIndex(m => m.GroupCallId);
    }
}
