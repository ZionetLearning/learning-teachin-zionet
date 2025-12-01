using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Accessor.Models.Lessons;
using Accessor.Models.Users;

namespace Accessor.DB.Configurations;

public class LessonsConfiguration : IEntityTypeConfiguration<LessonModel>
{
    public void Configure(EntityTypeBuilder<LessonModel> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.LessonId);

        builder.Property(l => l.LessonId)
            .HasColumnName("lesson_id")
            .IsRequired();

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(l => l.ContentSectionsJson)
            .HasColumnName("content_sections_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(l => l.TeacherId)
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.ModifiedAt)
            .HasColumnName("modified_at")
            .IsRequired();

        builder.HasIndex(l => l.TeacherId);
        builder.HasIndex(l => l.CreatedAt);

        builder.HasOne<UserModel>()
            .WithMany()
            .HasForeignKey(l => l.TeacherId)
            .HasConstraintName("FK_lessons_users_teacher_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
