using Accessor.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<UserModel>
{
    public void Configure(EntityTypeBuilder<UserModel> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.FirstName)
            .IsRequired();

        builder.Property(u => u.LastName)
            .IsRequired();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Role)
            .HasConversion<string>() // Store enum as string
            .IsRequired();

        builder.Property(u => u.PreferredLanguageCode)
            .HasConversion<string>() // enum → string
            .HasDefaultValue(SupportedLanguage.en) // default if not set
            .IsRequired();

        builder.Property(u => u.HebrewLevelValue)
            .HasConversion<string>() // nullable enum → string
            .IsRequired(false); // optional

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Role);
    }
}
