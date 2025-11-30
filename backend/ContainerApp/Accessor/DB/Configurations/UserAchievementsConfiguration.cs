using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Accessor.Models.Achievements;

namespace Accessor.DB.Configurations;

public class UserAchievementsConfiguration : IEntityTypeConfiguration<UserAchievementModel>
{
    public void Configure(EntityTypeBuilder<UserAchievementModel> builder)
    {
        builder.ToTable("UserAchievements");

        builder.HasKey(ua => ua.UserAchievementId);

        builder.Property(ua => ua.UserAchievementId)
            .HasColumnName("user_achievement_id")
            .IsRequired();

        builder.Property(ua => ua.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ua => ua.AchievementId)
            .HasColumnName("achievement_id")
            .IsRequired();

        builder.Property(ua => ua.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ua => ua.UnlockedAt)
            .HasColumnName("unlocked_at")
            .IsRequired();

        builder.HasIndex(ua => ua.UserId);
        builder.HasIndex(ua => ua.AchievementId);
        builder.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();

        builder.HasOne<AchievementModel>()
            .WithMany()
            .HasForeignKey(ua => ua.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
