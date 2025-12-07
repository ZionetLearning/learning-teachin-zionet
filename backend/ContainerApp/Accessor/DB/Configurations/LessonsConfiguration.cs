using System.Text.Json;
using Accessor.Models.Lessons;
using Accessor.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class LessonsConfiguration : IEntityTypeConfiguration<Lesson>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Configure(EntityTypeBuilder<Lesson> builder)
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

        builder.Property(l => l.ContentSections)
            .HasColumnName("content_sections_json")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<ContentSection>>(v, JsonOptions) ?? new List<ContentSection>(),
                new ValueComparer<List<ContentSection>>(
                    (c1, c2) => JsonSerializer.Serialize(c1, JsonOptions) == JsonSerializer.Serialize(c2, JsonOptions),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Heading.GetHashCode(), v.Body.GetHashCode())),
                    c => c.Select(cs => new ContentSection { Heading = cs.Heading, Body = cs.Body }).ToList()))
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
