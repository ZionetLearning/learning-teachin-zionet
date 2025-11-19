using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Accessor.Models.Achievements;

namespace Accessor.DB.Configurations;

public class AchievementsConfiguration : IEntityTypeConfiguration<AchievementModel>
{
    public void Configure(EntityTypeBuilder<AchievementModel> builder)
    {
        builder.ToTable("Achievements");

        builder.HasKey(a => a.AchievementId);

        builder.Property(a => a.AchievementId)
            .HasColumnName("achievement_id")
            .IsRequired();

        builder.Property(a => a.Key)
            .HasColumnName("key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(a => a.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.Feature)
            .HasColumnName("feature")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.TargetCount)
            .HasColumnName("target_count")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(a => a.Key).IsUnique();
        builder.HasIndex(a => a.Feature);
    }
}
