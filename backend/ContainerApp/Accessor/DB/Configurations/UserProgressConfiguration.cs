using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Accessor.Models.Achievements;

namespace Accessor.DB.Configurations;

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgressModel>
{
    public void Configure(EntityTypeBuilder<UserProgressModel> builder)
    {
        builder.ToTable("UserProgress");

        builder.HasKey(up => up.UserProgressId);

        builder.Property(up => up.UserProgressId)
            .HasColumnName("user_progress_id")
            .IsRequired();

        builder.Property(up => up.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(up => up.Feature)
            .HasColumnName("feature")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(up => up.Count)
            .HasColumnName("count")
            .IsRequired();

        builder.Property(up => up.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(up => up.UserId);
        builder.HasIndex(up => new { up.UserId, up.Feature }).IsUnique();
    }
}
