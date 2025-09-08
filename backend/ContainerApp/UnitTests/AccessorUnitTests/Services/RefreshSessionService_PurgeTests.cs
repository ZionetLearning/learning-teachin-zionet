﻿using System.Net;
using Accessor.DB;
using Accessor.Models;
using Accessor.Services;
using AccessorUnitTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Cleanup;

public class RefreshSessionService_PurgeTests
{

    private static RefreshSessionService NewService(AccessorDbContext db)
    {
        var logger = Mock.Of<ILogger<RefreshSessionService>>();
        return new RefreshSessionService(logger, db);
    }

    private static RefreshSessionsRecord Make(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        RefreshTokenHash = Guid.NewGuid().ToString("N"),
        DeviceFingerprintHash = "fp",
        IP = IPAddress.Parse("127.0.0.1"),
        UserAgent = "unit-test",
        IssuedAt = DateTimeOffset.UtcNow,
        LastSeenAt = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(60)
    };


    [Fact]
    public async Task Purge_NoRows_ReturnsZero()
    {
        var db = DbHelpers.NewInMemoryDb(Guid.NewGuid().ToString());
        var svc = NewService(db);

        var deleted = await svc.PurgeExpiredOrRevokedAsync(batchSize: 1000, ct: default);

        deleted.Should().Be(0);
        (await db.RefreshSessions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Purge_Deletes_Only_Expired()
    {
        var db = DbHelpers.NewInMemoryDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var expired1 = Make(); expired1.ExpiresAt = now.AddMinutes(-1);
        var expired2 = Make(); expired2.ExpiresAt = now.AddDays(-5);
        var fresh = Make(); fresh.ExpiresAt = now.AddDays(+10);

        db.RefreshSessions.AddRange(expired1, expired2, fresh);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var deleted = await svc.PurgeExpiredOrRevokedAsync(5000, default);

        deleted.Should().Be(2);
        (await db.RefreshSessions.FindAsync(fresh.Id)).Should().NotBeNull();
        (await db.RefreshSessions.FindAsync(expired1.Id)).Should().BeNull();
        (await db.RefreshSessions.FindAsync(expired2.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Purge_Deletes_Only_Revoked()
    {
        var db = DbHelpers.NewInMemoryDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var revoked = Make(); revoked.RevokedAt = now.AddMinutes(-2);
        var keep = Make(); keep.RevokedAt = null; keep.ExpiresAt = now.AddDays(+3);

        db.RefreshSessions.AddRange(revoked, keep);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var deleted = await svc.PurgeExpiredOrRevokedAsync(5000, default);

        deleted.Should().Be(1);
        (await db.RefreshSessions.FindAsync(keep.Id)).Should().NotBeNull();
        (await db.RefreshSessions.FindAsync(revoked.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Purge_Deletes_Expired_And_Revoked_Mixed()
    {
        var db = DbHelpers.NewInMemoryDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var expired = Make(); expired.ExpiresAt = now.AddSeconds(-1);
        var revoked = Make(); revoked.RevokedAt = now.AddHours(-1);
        var keep = Make(); keep.ExpiresAt = now.AddDays(+1);

        db.RefreshSessions.AddRange(expired, revoked, keep);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var deleted = await svc.PurgeExpiredOrRevokedAsync(5000, default);

        deleted.Should().Be(2);
        (await db.RefreshSessions.FindAsync(keep.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Purge_Runs_In_Batches()
    {
        var db = DbHelpers.NewInMemoryDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        // 13 expired + 7 fresh; batch size 5 => deletes across 3 loops (5,5,3)
        var expired = Enumerable.Range(0, 13).Select(i =>
        {
            var r = Make(); r.ExpiresAt = now.AddDays(-(i + 1)); return r;
        });
        var fresh = Enumerable.Range(0, 7).Select(i =>
        {
            var r = Make(); r.ExpiresAt = now.AddDays(i + 1); return r;
        });

        db.RefreshSessions.AddRange(expired);
        db.RefreshSessions.AddRange(fresh);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var deleted = await svc.PurgeExpiredOrRevokedAsync(batchSize: 5, ct: default);

        deleted.Should().Be(13);
        (await db.RefreshSessions.CountAsync()).Should().Be(7);
    }

    [Fact]
    public async Task Purge_Is_Idempotent_Second_Run_Removes_Zero()
    {
        var db = DbHelpers.NewInMemoryDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var e1 = Make(); e1.ExpiresAt = now.AddMinutes(-5);
        var e2 = Make(); e2.RevokedAt = now.AddMinutes(-1);
        db.RefreshSessions.AddRange(e1, e2);
        await db.SaveChangesAsync();

        var svc = NewService(db);

        var first = await svc.PurgeExpiredOrRevokedAsync(100, default);
        var second = await svc.PurgeExpiredOrRevokedAsync(100, default);

        first.Should().Be(2);
        second.Should().Be(0);
        (await db.RefreshSessions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Purge_Does_Not_Delete_Fresh_And_Not_Revoked()
    {
        var db = DbHelpers.NewInMemoryDb(Guid.NewGuid().ToString());
        var now = DateTimeOffset.UtcNow;

        var a = Make(); a.ExpiresAt = now.AddDays(+1);
        var b = Make(); b.ExpiresAt = now.AddMinutes(+1);
        var c = Make(); c.ExpiresAt = now.AddYears(+1);

        db.RefreshSessions.AddRange(a, b, c);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var deleted = await svc.PurgeExpiredOrRevokedAsync(1000, default);

        deleted.Should().Be(0);
        (await db.RefreshSessions.CountAsync()).Should().Be(3);
    }
}
