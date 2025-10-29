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

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.GroupCallId)
            .IsRequired();

        builder.Property(m => m.CreatedOn)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.Property(m => m.CreatedByUserId)
            .IsRequired();

        // Indexes for efficient querying
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.StartTimeUtc);
        builder.HasIndex(m => m.CreatedByUserId);
        builder.HasIndex(m => m.GroupCallId);
    }
}
