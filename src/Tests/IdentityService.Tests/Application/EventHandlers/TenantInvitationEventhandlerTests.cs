using DotNetCore.CAP;
using FluentAssertions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Status;
using IdentityService.Application.EventHandlers;
using IdentityService.Application.Events;
using IdentityService.Application.Services.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Events;
using SharedKernel.Topics;
using SharedKernel.Common.Results;
using Xunit;

namespace IdentityService.Tests.Application.EventHandlers;

public class TenantInvitationEventHandlerTests
{
    private readonly Mock<ITenantInvitationService> _tenantInvitationServiceMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<ICapPublisher> _capPublisherMock = new();
    private readonly Mock<ILogger<TenantInvitationEventHandler>> _loggerMock = new();

    private readonly TenantInvitationEventHandler _sut;

    public TenantInvitationEventHandlerTests()
    {
        _sut = new TenantInvitationEventHandler(
            _tenantInvitationServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _capPublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Not_Publish_When_Invitation_Not_Found()
    {
        var notification = new TenantInvitationCreatedEvent(
            Guid.NewGuid().ToString(),
            "FinTrack",
            "user@test.com");

        _tenantInvitationServiceMock
            .Setup(x => x.GetTenantInvitationByPublicIdAsync(
                notification.InvitationPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Fail(
                "INVITATION_NOT_FOUND",
                "Invitation not found."));

        await _sut.Handle(notification, CancellationToken.None);

        _capPublisherMock.Verify(
            x => x.PublishAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Not_Publish_When_Token_Generation_Fails()
    {
        var invitation = new TenantInvitation
        {
            PublicId = Guid.NewGuid(),
            Email = "user@test.com",
            TenantId = 1,
            Tenant = new Tenant
            {
                Id = 1,
                PublicId = Guid.NewGuid(),
                Name = "FinTrack"
            },
            Role = TenantRole.Member,
            Status = InvitationStatus.Pending,
            Version = 1,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedByUserId = 100
        };

        var notification = new TenantInvitationCreatedEvent(
            invitation.PublicId.ToString(),
            invitation.Tenant.Name,
            invitation.Email);

        _tenantInvitationServiceMock
            .Setup(x => x.GetTenantInvitationByPublicIdAsync(
                notification.InvitationPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(
                invitation,
                "INVITATION_FOUND",
                "Invitation found."));

        _jwtTokenServiceMock
            .Setup(x => x.GenerateInvitationTokenAsync(It.IsAny<InvitationClaimSource>()))
            .ReturnsAsync(ServiceResult<string>.Fail(
                "INVITATION_TOKEN_FAILED",
                "Token generation failed."));

        await _sut.Handle(notification, CancellationToken.None);

        _capPublisherMock.Verify(
            x => x.PublishAsync<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Publish_Email_Event_When_Invitation_And_Token_Are_Valid()
    {
        var invitation = new TenantInvitation
        {
            PublicId = Guid.NewGuid(),
            Email = "user@test.com",
            TenantId = 1,
            Tenant = new Tenant
            {
                Id = 1,
                PublicId = Guid.NewGuid(),
                Name = "FinTrack"
            },
            Role = TenantRole.Member,
            Status = InvitationStatus.Pending,
            Version = 1,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedByUserId = 100
        };

        var notification = new TenantInvitationCreatedEvent(
            invitation.PublicId.ToString(),
            invitation.Tenant.Name,
            invitation.Email);

        _tenantInvitationServiceMock
            .Setup(x => x.GetTenantInvitationByPublicIdAsync(
                notification.InvitationPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(
                invitation,
                "INVITATION_FOUND",
                "Invitation found."));

        _jwtTokenServiceMock
            .Setup(x => x.GenerateInvitationTokenAsync(It.IsAny<InvitationClaimSource>()))
            .ReturnsAsync(ServiceResult<string>.Ok(
                "fake-invitation-token",
                "INVITATION_TOKEN_SUCCESS",
                "Token generated."));

        await _sut.Handle(notification, CancellationToken.None);

        _capPublisherMock.Verify(
            x => x.PublishAsync(
                NotificationTopics.TenantInvitationEmailRequested,
                It.Is<TenantInvitationEmailRequestedEvent>(e =>
                    e.ToEmail == invitation.Email &&
                    e.TenantName == invitation.Tenant.Name &&
                    e.Role == invitation.Role.ToString() &&
                    e.InvitationLink.Contains("fake-invitation-token")),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_Should_Encode_Token_When_Building_Invitation_Link()
    {
        // Arrange
        var invitation = new TenantInvitation
        {
            PublicId = Guid.NewGuid(),
            Email = "user@test.com",
            TenantId = 1,
            Tenant = new Tenant
            {
                Id = 1,
                PublicId = Guid.NewGuid(),
                Name = "FinTrack"
            },
            Role = TenantRole.Member,
            Status = InvitationStatus.Pending,
            Version = 1,
            ExpiredAt = DateTime.UtcNow.AddDays(7),
            CreatedByUserId = 100
        };

        var rawToken = "abc+123/xyz==";

        var notification = new TenantInvitationCreatedEvent(
            invitation.PublicId.ToString(),
            invitation.Tenant.Name,
            invitation.Email);

        _tenantInvitationServiceMock
            .Setup(x => x.GetTenantInvitationByPublicIdAsync(
                notification.InvitationPublicId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantInvitation>.Ok(
                invitation,
                "INVITATION_FOUND",
                "Invitation found."));

        _jwtTokenServiceMock
            .Setup(x => x.GenerateInvitationTokenAsync(It.IsAny<InvitationClaimSource>()))
            .ReturnsAsync(ServiceResult<string>.Ok(
                rawToken,
                "INVITATION_TOKEN_SUCCESS",
                "Token generated."));

        var encodedToken = Uri.EscapeDataString(rawToken);

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        _capPublisherMock.Verify(
            x => x.PublishAsync(
                NotificationTopics.TenantInvitationEmailRequested,
                It.Is<TenantInvitationEmailRequestedEvent>(e =>
                    e.ToEmail == invitation.Email &&
                    e.TenantName == invitation.Tenant.Name &&
                    e.Role == invitation.Role.ToString() &&
                    e.InvitationLink.Contains(encodedToken)),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}