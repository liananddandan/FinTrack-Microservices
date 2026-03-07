using AutoFixture;
using FluentAssertions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Services;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using IdentityService.Infrastructure.Persistence.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Common.Results;

namespace IdentityService.Tests.Application.Services;

public class TenantServiceTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<ILogger<TenantService>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantRepo> _tenantRepoMock = new();
    private readonly Mock<IApplicationUserRepo> _applicationUserRepoMock = new();
    private readonly Mock<ITenantMembershipRepo> _tenantMembershipRepoMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();

    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    private readonly TenantService _sut;

    public TenantServiceTests()
    {
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
            _mediatorMock.Object);
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
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Message.Should().Be(expectedMessage);
        result.Code.Should().Be(ResultCodes.Tenant.RegisterTenantParameterError);

        _tenantRepoMock.Verify(
            x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.WithTransactionAsync(It.IsAny<Func<Task<ServiceResult<RegisterTenantResult>>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Tenant_Name_Already_Exists()
    {
        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.WithTransactionAsync(It.IsAny<Func<Task<ServiceResult<RegisterTenantResult>>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<ServiceResult<RegisterTenantResult>>>, CancellationToken>((action, _) => action());

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Tenant.RegisterTenantExistedError);
        result.Message.Should().Be("Tenant name already exists.");

        _applicationUserRepoMock.Verify(
            x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _tenantRepoMock.Verify(
            x => x.AddTenantAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Admin_Email_Already_Exists()
    {
        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock
            .Setup(x => x.WithTransactionAsync(It.IsAny<Func<Task<ServiceResult<RegisterTenantResult>>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<ServiceResult<RegisterTenantResult>>>, CancellationToken>((action, _) => action());

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Tenant.RegisterTenantExistedError);
        result.Message.Should().Be("Admin email already exists.");

        _tenantRepoMock.Verify(
            x => x.AddTenantAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _userManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_UserManager_CreateAsync_Fails()
    {
        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.WithTransactionAsync(It.IsAny<Func<Task<ServiceResult<RegisterTenantResult>>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<ServiceResult<RegisterTenantResult>>>, CancellationToken>((action, _) => action());

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
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Tenant.RegisterTenantCreateError);
        result.Message.Should().Contain("Password is too weak.");

        _tenantMembershipRepoMock.Verify(
            x => x.AddMembershipAsync(It.IsAny<TenantMembership>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Create_Tenant_User_And_Membership_When_Request_Is_Valid()
    {
        var tenantName = "FinTrack";
        var adminName = "Emily";
        var adminEmail = "Emily@Test.com";
        var adminPassword = "Password123!";

        _tenantRepoMock
            .Setup(x => x.IsTenantNameExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _applicationUserRepoMock
            .Setup(x => x.IsEmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(x => x.WithTransactionAsync(It.IsAny<Func<Task<ServiceResult<RegisterTenantResult>>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<ServiceResult<RegisterTenantResult>>>, CancellationToken>((action, _) => action());

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        Tenant? addedTenant = null;
        _tenantRepoMock
            .Setup(x => x.AddTenantAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Tenant, CancellationToken>((tenant, _) =>
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
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Tenant.RegisterTenantSuccess);
        result.Message.Should().Be("Tenant created successfully.");
        result.Data.Should().NotBeNull();

        addedTenant.Should().NotBeNull();
        addedTenant!.Name.Should().Be(tenantName);

        createdUser.Should().NotBeNull();
        createdUser!.Email.Should().Be(adminEmail.Trim().ToLowerInvariant());
        createdUser.UserName.Should().Be(adminEmail.Trim().ToLowerInvariant());
        createdUser.EmailConfirmed.Should().BeTrue();

        addedMembership.Should().NotBeNull();
        addedMembership!.TenantId.Should().Be(addedTenant.Id);
        addedMembership.UserId.Should().Be(createdUser.Id);
        addedMembership.Role.Should().Be(TenantRole.Admin);
        addedMembership.IsActive.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _tenantRepoMock.Verify(x => x.AddTenantAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        _tenantMembershipRepoMock.Verify(x => x.AddMembershipAsync(It.IsAny<TenantMembership>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterTenantAsync_Should_Return_Fail_When_Exception_Is_Thrown()
    {
        _unitOfWorkMock
            .Setup(x => x.WithTransactionAsync(It.IsAny<Func<Task<ServiceResult<RegisterTenantResult>>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("unexpected"));

        var result = await _sut.RegisterTenantAsync(
            "FinTrack",
            "Emily",
            "emily@test.com",
            "Password123!",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Tenant.RegisterTenantException);
        result.Message.Should().Be("Tenant registration failed.");
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
            Tenant = new Tenant
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
            Tenant = new Tenant
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
}