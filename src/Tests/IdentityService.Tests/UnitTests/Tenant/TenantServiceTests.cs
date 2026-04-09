using AutoFixture;
using FluentAssertions;
using IdentityService.Application.Accounts.Abstractions;
using IdentityService.Application.Accounts.Dtos;
using IdentityService.Application.Accounts.Events;
using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Tenants.Abstractions;
using IdentityService.Application.Tenants.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Common.Results;
using StackExchange.Redis;

namespace IdentityService.Tests.UnitTests.Tenant;

public class TenantServiceTests
{
    private const string ValidTurnstileToken = "valid-turnstile-token";

    private readonly Fixture _fixture = new();

    private readonly Mock<ILogger<TenantService>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IApplicationUserRepo> _applicationUserRepoMock = new();
    private readonly Mock<ITenantMembershipRepo> _tenantMembershipRepoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IAuditLogPublisher> _auditLogPublisherMock = new();
    private readonly Mock<ITurnstileValidationService> _turnstileValidationServiceMock = new();
    private readonly Mock<IEmailThrottleService> _emailThrottleServiceMock = new();
    private readonly Mock<IEmailVerificationService> _emailVerificationServiceMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock = new();

    private readonly TenantService _sut;

    public TenantServiceTests()
    {
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        _sut = new TenantService(
            _loggerMock.Object,
            _unitOfWorkMock.Object,
            _tenantRepoMock.Object,
            _applicationUserRepoMock.Object,
            _userManagerMock.Object,
            _tenantMembershipRepoMock.Object,
            _connectionMultiplexerMock.Object,
            _auditLogPublisherMock.Object,
            _turnstileValidationServiceMock.Object,
            _emailThrottleServiceMock.Object,
            _mediatorMock.Object,
            _emailVerificationServiceMock.Object
        );
    }

    [Theory]
    [InlineData("", "admin", "admin@test.com", "Password123!", "Tenant name is required.")]
    [InlineData("tenant", "", "admin@test.com", "Password123!", "Admin name is required.")]
    [InlineData("tenant", "admin", "", "Password123!", "Admin email is required.")]
    [InlineData("tenant", "admin", "admin@test.com", "", "Admin password is required.")]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Required_Parameter_Is_Missing(
        string tenantName,
        string adminName,
        string adminEmail,
        string adminPassword,
        string expectedMessage)
    {
        var result = await _sut.RegisterTenantAsync(
            tenantName,
            adminName,
            adminEmail,
            adminPassword,
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
        result.Code.Should().Be(ResultCodes.TenantCodes.RegisterTenantParameterError);

        _turnstileValidationServiceMock.Verify(
            x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _tenantRepoMock.Verify(
            x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Turnstile_Token_Is_Missing()
    {
        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            "",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TURNSTILE_TOKEN_REQUIRED");
        result.Message.Should().Be("Verification challenge is required.");

        _turnstileValidationServiceMock.Verify(
            x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Turnstile_Validation_Fails()
    {
        _turnstileValidationServiceMock
            .Setup(x => x.ValidateAsync(
                ValidTurnstileToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Fail(
                "TURNSTILE_VERIFY_FAILED",
                "Verification failed. Please try again."));

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be("TURNSTILE_VERIFY_FAILED");
        result.Message.Should().Be("Verification failed. Please try again.");

        _tenantRepoMock.Verify(
            x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Tenant_Name_Already_Exists()
    {
        _turnstileValidationServiceMock
            .Setup(x => x.ValidateAsync(
                ValidTurnstileToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "TURNSTILE_VERIFY_SUCCESS",
                "Verification passed."));

        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.RegisterTenantExistedError);
        result.Message.Should().Be("Tenant name already exists.");

        _applicationUserRepoMock.Verify(
            x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _tenantRepoMock.Verify(
            x => x.AddTenantAsync(It.IsAny<Domain.Entities.Tenant>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Admin_Email_Already_Exists()
    {
        _turnstileValidationServiceMock
            .Setup(x => x.ValidateAsync(
                ValidTurnstileToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "TURNSTILE_VERIFY_SUCCESS",
                "Verification passed."));

        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.RegisterTenantExistedError);
        result.Message.Should().Be("Admin email already exists.");

        _tenantRepoMock.Verify(
            x => x.AddTenantAsync(It.IsAny<Domain.Entities.Tenant>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_UserManager_CreateAsync_Fails()
    {
        _turnstileValidationServiceMock
            .Setup(x => x.ValidateAsync(
                ValidTurnstileToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "TURNSTILE_VERIFY_SUCCESS",
                "Verification passed."));

        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError
            {
                Description = "Password is too weak."
            }));

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.RegisterTenantCreateError);
        result.Message.Should().Contain("Password is too weak.");

        _tenantMembershipRepoMock.Verify(
            x => x.AddMembershipAsync(It.IsAny<TenantMembership>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task
        RegisterTenantAsync_Should_Create_Tenant_User_And_Membership_And_Send_Verification_Email_When_Request_Is_Valid()
    {
        const string tenantName = "FinTrack";
        const string adminName = "Emily";
        const string adminEmail = "Emily@Test.com";
        const string adminPassword = "Password123!";

        _turnstileValidationServiceMock
            .Setup(x => x.ValidateAsync(
                ValidTurnstileToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "TURNSTILE_VERIFY_SUCCESS",
                "Verification passed."));

        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _emailThrottleServiceMock
            .Setup(x => x.CheckRegistrationEmailSendAllowedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "REGISTRATION_EMAIL_ALLOWED",
                "Registration verification email can be sent."));

        _emailThrottleServiceMock
            .Setup(x => x.MarkRegistrationEmailSentAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailVerificationServiceMock
            .Setup(x => x.CreateTokenAsync(It.IsAny<long>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<CreateEmailVerificationTokenResult>.Ok(
                new CreateEmailVerificationTokenResult("raw-token", DateTime.UtcNow.AddHours(24)),
                "EMAIL_VERIFICATION_TOKEN_CREATED",
                "Token created successfully."));

        _mediatorMock
            .Setup(x => x.Publish(It.IsAny<SendEmailVerificationRequestedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Domain.Entities.Tenant? addedTenant = null;
        _tenantRepoMock
            .Setup(x => x.AddTenantAsync(It.IsAny<Domain.Entities.Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Tenant, CancellationToken>((tenant, _) =>
            {
                tenant.Id = 100;
                tenant.PublicId = Guid.NewGuid();
                addedTenant = tenant;
            })
            .Returns(Task.CompletedTask);

        TenantMembership? addedMembership = null;
        _tenantMembershipRepoMock
            .Setup(x => x.AddMembershipAsync(It.IsAny<TenantMembership>(), It.IsAny<CancellationToken>()))
            .Callback<TenantMembership, CancellationToken>((membership, _) => addedMembership = membership)
            .Returns(Task.CompletedTask);

        ApplicationUser? createdUser = null;
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), adminPassword))
            .Callback<ApplicationUser, string>((user, _) =>
            {
                user.Id = 200;
                user.PublicId = Guid.NewGuid();
                createdUser = user;
            })
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterTenantAsync(
            tenantName,
            adminName,
            adminEmail,
            adminPassword,
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.TenantCodes.RegisterTenantSuccess);
        result.Message.Should()
            .Be("Tenant created successfully. Please verify the admin email before accessing the workspace.");
        result.Data.Should().NotBeNull();

        addedTenant.Should().NotBeNull();
        addedTenant!.Name.Should().Be(tenantName);

        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be(adminEmail.Trim().ToLowerInvariant());
        createdUser.UserName.Should().Be(adminEmail.Trim().ToLowerInvariant());
        createdUser.EmailConfirmed.Should().BeFalse();

        addedMembership.Should().NotBeNull();
        addedMembership!.TenantId.Should().Be(addedTenant.Id);
        addedMembership.UserId.Should().Be(createdUser.Id);
        addedMembership.Role.Should().Be(TenantRole.Admin);
        addedMembership.IsActive.Should().BeTrue();

        _tenantRepoMock.Verify(
            x => x.AddTenantAsync(It.IsAny<Domain.Entities.Tenant>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _tenantMembershipRepoMock.Verify(
            x => x.AddMembershipAsync(It.IsAny<TenantMembership>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _emailVerificationServiceMock.Verify(
            x => x.CreateTokenAsync(createdUser.Id, null, It.IsAny<CancellationToken>()),
            Times.Once);

        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<SendEmailVerificationRequestedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _emailThrottleServiceMock.Verify(
            x => x.MarkRegistrationEmailSentAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task
        RegisterTenantAsync_Should_Create_Tenant_User_And_Membership_But_Skip_Email_When_Registration_Email_Is_Throttled()
    {
        _turnstileValidationServiceMock
            .Setup(x => x.ValidateAsync(
                ValidTurnstileToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "TURNSTILE_VERIFY_SUCCESS",
                "Verification passed."));

        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _emailThrottleServiceMock
            .Setup(x => x.CheckRegistrationEmailSendAllowedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Fail(
                "REGISTRATION_EMAIL_THROTTLED",
                "Registration succeeded, but verification email was temporarily delayed due to high traffic."));

        _tenantRepoMock
            .Setup(x => x.AddTenantAsync(It.IsAny<Domain.Entities.Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Tenant, CancellationToken>((tenant, _) =>
            {
                tenant.Id = 100;
                tenant.PublicId = Guid.NewGuid();
            })
            .Returns(Task.CompletedTask);

        _tenantMembershipRepoMock
            .Setup(x => x.AddMembershipAsync(It.IsAny<TenantMembership>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.TenantCodes.RegisterTenantSuccess);
        result.Message.Should()
            .Be(
                "Tenant created successfully. Verification email was temporarily delayed. Please log in later to resend verification email.");

        _emailVerificationServiceMock.Verify(
            x => x.CreateTokenAsync(It.IsAny<long>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<SendEmailVerificationRequestedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _emailThrottleServiceMock.Verify(
            x => x.MarkRegistrationEmailSentAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        _turnstileValidationServiceMock
            .Setup(x => x.ValidateAsync(
                ValidTurnstileToken,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<bool>.Ok(
                true,
                "TURNSTILE_VERIFY_SUCCESS",
                "Verification passed."));

        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("unexpected"));

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            ValidTurnstileToken,
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.RegisterTenantException);
        result.Message.Should().Be("Tenant registration failed.");

        _unitOfWorkMock.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task GetTenantMembersAsync_Should_Return_Fail_When_TenantPublicId_Is_Empty()
    {
        var result = await _sut.GetTenantMembersAsync("", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Tenant public id is required.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetTenantMembersAsync_Should_Return_Empty_List_When_No_Members_Exist()
    {
        _tenantMembershipRepoMock
            .Setup(x => x.GetMembershipsByTenantPublicIdAsync("tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantMembership>());

        var result = await _sut.GetTenantMembersAsync("tenant-1", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Tenant members fetched successfully.");
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTenantMembersAsync_Should_Return_Mapped_Member_List_When_Request_Is_Valid()
    {
        var membership1 = new TenantMembership
        {
            IsActive = true,
            JoinedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            Role = TenantRole.Admin,
            User = new ApplicationUser
            {
                PublicId = Guid.NewGuid(),
                Email = "admin@test.com",
                UserName = "AdminUser"
            },
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = Guid.NewGuid(),
                Name = "FinTrack",
                IsDeleted = false
            }
        };

        var membership2 = new TenantMembership
        {
            IsActive = true,
            JoinedAt = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc),
            Role = TenantRole.Member,
            User = new ApplicationUser
            {
                PublicId = Guid.NewGuid(),
                Email = "member@test.com",
                UserName = "MemberUser"
            },
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = Guid.NewGuid(),
                Name = "FinTrack",
                IsDeleted = false
            }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetMembershipsByTenantPublicIdAsync("tenant-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantMembership> { membership1, membership2 });

        var result = await _sut.GetTenantMembersAsync("tenant-1", CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        result.Data![0].Email.Should().Be("admin@test.com");
        result.Data[0].UserName.Should().Be("AdminUser");
        result.Data[0].Role.Should().Be("Admin");
        result.Data[0].IsActive.Should().BeTrue();

        result.Data[1].Email.Should().Be("member@test.com");
        result.Data[1].UserName.Should().Be("MemberUser");
        result.Data[1].Role.Should().Be("Member");
        result.Data[1].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetTenantMembersAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        _tenantMembershipRepoMock
            .Setup(x => x.GetMembershipsByTenantPublicIdAsync("tenant-1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.GetTenantMembersAsync("tenant-1", CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Failed to get tenant members.");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task RemoveTenantMemberAsync_Should_Return_Fail_When_Membership_NotFound()
    {
        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantMembership?)null);

        var result = await _sut.RemoveTenantMemberAsync(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.MemberNotFound);
    }

    [Fact]
    public async Task RemoveTenantMemberAsync_Should_Return_Fail_When_Member_Already_Removed()
    {
        var membership = _fixture.Build<TenantMembership>()
            .With(x => x.IsActive, false)
            .Create();

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var result = await _sut.RemoveTenantMemberAsync(
            membership.Tenant.PublicId.ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.MemberAlreadyRemoved);
    }

    [Fact]
    public async Task RemoveTenantMemberAsync_Should_Return_Fail_When_Member_Not_In_Tenant()
    {
        var membership = _fixture.Build<TenantMembership>()
            .With(x => x.IsActive, true)
            .Create();

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var result = await _sut.RemoveTenantMemberAsync(
            Guid.NewGuid().ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.MemberNotInTenant);
    }

    [Fact]
    public async Task RemoveTenantMemberAsync_Should_Return_Fail_When_Remove_Self()
    {
        var userId = Guid.NewGuid();

        var membership = _fixture.Build<TenantMembership>()
            .With(x => x.IsActive, true)
            .With(x => x.User, new ApplicationUser { PublicId = userId })
            .Create();

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var result = await _sut.RemoveTenantMemberAsync(
            membership.Tenant.PublicId.ToString(),
            membership.PublicId.ToString(),
            userId.ToString(),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.CannotRemoveSelf);
    }

    [Fact]
    public async Task RemoveTenantMemberAsync_Should_Succeed_When_Valid()
    {
        var membership = _fixture.Build<TenantMembership>()
            .With(x => x.IsActive, true)
            .Create();

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var redisDbMock = new Mock<IDatabase>();

        _connectionMultiplexerMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(redisDbMock.Object);

        var result = await _sut.RemoveTenantMemberAsync(
            membership.Tenant.PublicId.ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        result.Success.Should().BeTrue();

        membership.IsActive.Should().BeFalse();
        membership.LeftAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        redisDbMock.Verify(
            x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Return_Fail_When_Role_Is_Invalid()
    {
        var result = await _sut.ChangeTenantMemberRoleAsync(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "SuperAdmin",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.ChangeMemberRoleInvalidRole);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Return_Fail_When_Membership_NotFound()
    {
        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantMembership?)null);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "Admin",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.MemberNotFound);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Return_Fail_When_Member_Not_In_Tenant()
    {
        var tenantId = Guid.NewGuid();
        var membership = new TenantMembership
        {
            PublicId = Guid.NewGuid(),
            IsActive = true,
            Role = TenantRole.Member,
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = tenantId,
                Name = $"tenant-{tenantId}"
            },
            User = new ApplicationUser { PublicId = Guid.NewGuid() }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            Guid.NewGuid().ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            "Admin",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.MemberNotInTenant);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Return_Fail_When_Membership_Is_Inactive()
    {
        var tenantId = Guid.NewGuid();

        var membership = new TenantMembership
        {
            PublicId = Guid.NewGuid(),
            IsActive = false,
            Role = TenantRole.Member,
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = tenantId,
                Name = $"tenant-{tenantId}"
            },
            User = new ApplicationUser { PublicId = Guid.NewGuid() }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            tenantId.ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            "Admin",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.ChangeMemberRoleInactiveMembership);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Return_Fail_When_Changing_Own_Role()
    {
        var tenantId = Guid.NewGuid();
        var operatorUserId = Guid.NewGuid();

        var membership = new TenantMembership
        {
            PublicId = Guid.NewGuid(),
            IsActive = true,
            Role = TenantRole.Admin,
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = tenantId,
                Name = $"tenant-{tenantId}"
            },
            User = new ApplicationUser { PublicId = operatorUserId }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            tenantId.ToString(),
            membership.PublicId.ToString(),
            operatorUserId.ToString(),
            "Member",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.CannotChangeOwnRole);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Return_Fail_When_Demoting_Last_Admin()
    {
        var tenantId = Guid.NewGuid();

        var membership = new TenantMembership
        {
            PublicId = Guid.NewGuid(),
            IsActive = true,
            Role = TenantRole.Admin,
            TenantId = 100,
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = tenantId,
                Name = $"tenant-{tenantId}"
            },
            User = new ApplicationUser { PublicId = Guid.NewGuid() }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        _tenantMembershipRepoMock
            .Setup(x => x.CountActiveAdminsAsync(membership.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            tenantId.ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            "Member",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.TenantCodes.CannotDemoteLastAdmin);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Return_Ok_When_No_Change_Is_Required()
    {
        var tenantId = Guid.NewGuid();

        var membership = new TenantMembership
        {
            PublicId = Guid.NewGuid(),
            IsActive = true,
            Role = TenantRole.Member,
            TenantId = 100,
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = tenantId,
                Name = $"tenant-{tenantId}"
            },
            User = new ApplicationUser { PublicId = Guid.NewGuid() }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            tenantId.ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            "Member",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.TenantCodes.ChangeMemberRoleNoChange);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Promote_Member_To_Admin()
    {
        var tenantId = Guid.NewGuid();

        var membership = new TenantMembership
        {
            PublicId = Guid.NewGuid(),
            IsActive = true,
            Role = TenantRole.Member,
            TenantId = 100,
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = tenantId,
                Name = $"tenant-{tenantId}"
            },
            User = new ApplicationUser { PublicId = Guid.NewGuid() }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var redisDbMock = new Mock<IDatabase>();
        _connectionMultiplexerMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(redisDbMock.Object);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            tenantId.ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            "Admin",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.TenantCodes.ChangeMemberRoleSuccess);
        membership.Role.Should().Be(TenantRole.Admin);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        redisDbMock.Verify(
            x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeTenantMemberRoleAsync_Should_Demote_Admin_When_Not_Last_Admin()
    {
        var tenantId = Guid.NewGuid();

        var membership = new TenantMembership
        {
            PublicId = Guid.NewGuid(),
            IsActive = true,
            Role = TenantRole.Admin,
            TenantId = 100,
            Tenant = new Domain.Entities.Tenant
            {
                PublicId = tenantId,
                Name = $"tenant-{tenantId}"
            },
            User = new ApplicationUser { PublicId = Guid.NewGuid() }
        };

        _tenantMembershipRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        _tenantMembershipRepoMock
            .Setup(x => x.CountActiveAdminsAsync(membership.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var redisDbMock = new Mock<IDatabase>();
        _connectionMultiplexerMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(redisDbMock.Object);

        var result = await _sut.ChangeTenantMemberRoleAsync(
            tenantId.ToString(),
            membership.PublicId.ToString(),
            Guid.NewGuid().ToString(),
            "Member",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.TenantCodes.ChangeMemberRoleSuccess);
        membership.Role.Should().Be(TenantRole.Member);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        redisDbMock.Verify(
            x => x.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()),
            Times.Once);
    }
}