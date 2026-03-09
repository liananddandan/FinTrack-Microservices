using AuditLogService.Application.DTOs;
using AuditLogService.Application.Services;
using AuditLogService.Domain.Entities;
using AuditLogService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AuditLogService.Tests.Application.Services;

public class AuditLogReaderTests
{
    [Fact]
    public async Task QueryAsync_Should_Filter_By_Tenant_And_ActionType()
    {
        var options = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AuditLogDbContext(options);

        dbContext.AuditLogs.AddRange(
            new AuditLogEntry
            {
                TenantPublicId = "tenant-001",
                ActionType = "Membership.Invited",
                Category = "Membership",
                MetadataJson = "[]",
                OccurredAtUtc = DateTime.UtcNow.AddMinutes(-10)
            },
            new AuditLogEntry
            {
                TenantPublicId = "tenant-001",
                ActionType = "Membership.Removed",
                Category = "Membership",
                MetadataJson = "[]",
                OccurredAtUtc = DateTime.UtcNow.AddMinutes(-5)
            },
            new AuditLogEntry
            {
                TenantPublicId = "tenant-002",
                ActionType = "Membership.Invited",
                Category = "Membership",
                MetadataJson = "[]",
                OccurredAtUtc = DateTime.UtcNow.AddMinutes(-1)
            });

        await dbContext.SaveChangesAsync();

        var sut = new AuditLogReader(dbContext);

        var result = await sut.QueryAsync(
            new AuditLogQueryRequest
            {
                TenantPublicId = "tenant-001",
                ActionType = "Membership.Invited",
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].ActionType.Should().Be("Membership.Invited");
        result.Items[0].TenantPublicId.Should().Be("tenant-001");
    }

    [Fact]
    public async Task QueryAsync_Should_Order_By_OccurredAtUtc_Descending()
    {
        var options = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AuditLogDbContext(options);

        dbContext.AuditLogs.AddRange(
            new AuditLogEntry
            {
                TenantPublicId = "tenant-001",
                ActionType = "Membership.Invited",
                Category = "Membership",
                MetadataJson = "[]",
                TargetDisplay = "older@test.com",
                OccurredAtUtc = DateTime.UtcNow.AddMinutes(-10)
            },
            new AuditLogEntry
            {
                TenantPublicId = "tenant-001",
                ActionType = "Membership.Removed",
                Category = "Membership",
                MetadataJson = "[]",
                TargetDisplay = "newer@test.com",
                OccurredAtUtc = DateTime.UtcNow.AddMinutes(-1)
            });

        await dbContext.SaveChangesAsync();

        var sut = new AuditLogReader(dbContext);

        var result = await sut.QueryAsync(
            new AuditLogQueryRequest
            {
                TenantPublicId = "tenant-001",
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items[0].TargetDisplay.Should().Be("newer@test.com");
        result.Items[1].TargetDisplay.Should().Be("older@test.com");
    }

    [Fact]
    public async Task QueryAsync_Should_Apply_Paging()
    {
        var options = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AuditLogDbContext(options);

        for (var i = 1; i <= 5; i++)
        {
            dbContext.AuditLogs.Add(new AuditLogEntry
            {
                TenantPublicId = "tenant-001",
                ActionType = "Membership.Invited",
                Category = "Membership",
                MetadataJson = "[]",
                TargetDisplay = $"user-{i}@test.com",
                OccurredAtUtc = DateTime.UtcNow.AddMinutes(-i)
            });
        }

        await dbContext.SaveChangesAsync();

        var sut = new AuditLogReader(dbContext);

        var result = await sut.QueryAsync(
            new AuditLogQueryRequest
            {
                TenantPublicId = "tenant-001",
                PageNumber = 2,
                PageSize = 2
            },
            CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);
    }
}