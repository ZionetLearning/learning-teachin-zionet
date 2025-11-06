using Accessor.Models.GameConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class UserGameConfigConfiguration : IEntityTypeConfiguration<UserGameConfig>
{
    public void Configure(EntityTypeBuilder<UserGameConfig> builder)
    {
        builder.ToTable("UserGameConfigs");

        builder.HasKey(x => new { x.UserId, x.GameName });

        builder.Property(x => x.GameName)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(x => x.Difficulty)
               .HasConversion<string>()
               .IsRequired();

        builder.Property(x => x.Nikud)
               .IsRequired();

        builder.Property(x => x.NumberOfSentences)
               .IsRequired();
    }
}
