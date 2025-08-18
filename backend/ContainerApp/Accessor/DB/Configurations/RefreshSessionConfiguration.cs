using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Accessor.Models;

namespace Accessor.DB.Configurations;

public class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSessionsRecord>
{
    public void Configure(EntityTypeBuilder<RefreshSessionsRecord> builder)
    {
        builder.ToTable("refreshSessions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id").IsRequired();
        builder.Property(r => r.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(r => r.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(128).IsRequired();
        builder.Property(r => r.DeviceFingerprintHash).HasColumnName("device_fingerprint_hash").HasMaxLength(128).IsRequired(false);
        builder.Property(r => r.IssuedAt)
            .HasColumnName("issued_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(r => r.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW() + INTERVAL '60 days'")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(r => r.LastSeenAt)
            .HasColumnName("last_seen_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(r => r.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamptz");
        builder.Property(r => r.IP)
                .HasColumnName("ip")
                .HasColumnType("inet")
                .IsRequired();
        builder.Property(r => r.UserAgent).HasColumnName("user_agent").HasMaxLength(512).IsRequired();

        // Indexes
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.RefreshTokenHash).IsUnique();
        builder.HasIndex(r => r.DeviceFingerprintHash);
        builder.HasIndex(r => r.ExpiresAt);
        builder.HasIndex(r => r.RevokedAt);
    }
}
