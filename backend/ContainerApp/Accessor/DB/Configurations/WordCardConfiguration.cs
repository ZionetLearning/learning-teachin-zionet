using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Accessor.Models.WordCards;

namespace Accessor.DB.Configurations;

public class WordCardConfiguration : IEntityTypeConfiguration<WordCardModel>
{
    public void Configure(EntityTypeBuilder<WordCardModel> builder)
    {
        builder.ToTable("word_cards");

        builder.HasKey(w => w.CardId);

        builder.Property(w => w.CardId)
            .HasColumnName("card_id")
            .IsRequired();

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(w => w.Hebrew)
            .HasColumnName("hebrew")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.English)
            .HasColumnName("english")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.IsLearned)
            .HasColumnName("is_learned")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(w => new { w.UserId, w.IsLearned });
    }
}
