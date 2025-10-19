using Accessor.Models.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accessor.DB.Configurations;

public class ClassesConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");
        builder.HasKey(c => c.ClassId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_Classes_Name_CI");

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");
    }
}

public class ClassMembershipConfiguration : IEntityTypeConfiguration<ClassMembership>
{
    public void Configure(EntityTypeBuilder<ClassMembership> builder)
    {
        builder.ToTable("ClassMemberships");

        builder.HasKey(cm => new { cm.ClassId, cm.UserId, cm.Role });

        builder.HasOne(cm => cm.Class)
            .WithMany(c => c.Memberships)
            .HasForeignKey(cm => cm.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.User)
            .WithMany()
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cm => new { cm.UserId, cm.Role, cm.ClassId });
        builder.HasIndex(cm => new { cm.ClassId, cm.Role, cm.UserId });
    }
}
