using Accessor.Models.Prompts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public sealed class PromptConfiguration : IEntityTypeConfiguration<PromptModel>
{
    public void Configure(EntityTypeBuilder<PromptModel> builder)
    {
        builder.ToTable("Prompts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PromptKey)
               .IsRequired()
               .HasMaxLength(120);

        builder.Property(p => p.Version)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(p => p.Content)
               .IsRequired();

        builder.HasIndex(p => new { p.PromptKey, p.Version })
               .IsUnique()
               .IsDescending(false, true);
    }
}