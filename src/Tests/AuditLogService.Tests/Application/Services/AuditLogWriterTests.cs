using System.Text.Json;
using AuditLogService.Application.Services;
using AuditLogService.Domain.Entities;
using AuditLogService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Contracts.AuditLogs;
using Xunit;

namespace AuditLogService.Tests.Application.Services;

public class AuditLogWriterTests
{
    [Fact]
    public async Task WriteAsync_Should_Persist_AuditLogEntry()
    {
        var options = new DbContextOptionsBuilder<AuditLogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AuditLogDbContext(options);
        var sut = new AuditLogWriter(dbContext);

        var message = new AuditLogMessage
        {
            TenantPublicId = "tenant-001",
            ActorUserPublicId = "user-001",
            ActorDisplayName = "Emily",
            ActionType = "Membership.Invited",
            Category = "Membership",
            TargetType = "Invitation",
            TargetPublicId = "invitation-001",
            TargetDisplay = "foo@test.com",
            Source = "IdentityService",
            CorrelationId = "corr-001",
            OccurredAtUtc = DateTime.UtcNow,
            Metadata =
            [
                new AuditMetadataItem("role", "Member"),
                new AuditMetadataItem("email", "foo@test.com")
            ]
        };

        await sut.WriteAsync(message, CancellationToken.None);

        var entry = await dbContext.AuditLogs.SingleAsync();

        entry.TenantPublicId.Should().Be("tenant-001");
        entry.ActorUserPublicId.Should().Be("user-001");
        entry.ActorDisplayName.Should().Be("Emily");
        entry.ActionType.Should().Be("Membership.Invited");
        entry.Category.Should().Be("Membership");
        entry.TargetType.Should().Be("Invitation");
        entry.TargetPublicId.Should().Be("invitation-001");
        entry.TargetDisplay.Should().Be("foo@test.com");
        entry.Source.Should().Be("IdentityService");
        entry.CorrelationId.Should().Be("corr-001");

        entry.MetadataJson.Should().NotBeNullOrWhiteSpace();

        var metadata = JsonSerializer.Deserialize<List<AuditMetadataItem>>(entry.MetadataJson);
        metadata.Should().NotBeNull();
        metadata!.Should().Contain(x => x.Key == "role" && x.Value == "Member");
        metadata.Should().Contain(x => x.Key == "email" && x.Value == "foo@test.com");
    }
}