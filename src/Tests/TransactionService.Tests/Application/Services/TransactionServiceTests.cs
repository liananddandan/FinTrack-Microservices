using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;
using TransactionService.Application.Services.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence.Repositories.Interfaces;

namespace TransactionService.Tests.Application.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepo> _transactionRepoMock = new();
    private readonly Mock<ITenantAccountRepo> _tenantAccountRepoMock = new();
    private readonly Mock<ITenantInfoClient> _tenantInfoClientMock = new();
    private readonly Mock<IPaymentGateway> _paymentGatewayMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<TransactionService.Application.Services.TransactionService>> _loggerMock = new();

    private readonly TransactionService.Application.Services.TransactionService _sut;

    public TransactionServiceTests()
    {
        _sut = new TransactionService.Application.Services.TransactionService(
            _transactionRepoMock.Object,
            _tenantAccountRepoMock.Object,
            _tenantInfoClientMock.Object,
            _paymentGatewayMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateDonationAsync_Should_Return_Fail_When_Amount_Is_Invalid()
    {
        var result = await _sut.CreateDonationAsync(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            "Donation",
            "desc",
            0,
            "NZD",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionCreateFailed);
        result.Message.Should().Be("Amount must be greater than zero.");

        _transactionRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateDonationAsync_Should_Return_Fail_When_Tenant_Info_Cannot_Be_Resolved()
    {
        var tenantPublicId = Guid.NewGuid().ToString();
        var userPublicId = Guid.NewGuid().ToString();

        _tenantInfoClientMock
            .Setup(x => x.GetTenantSummaryAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantSummaryDto>.Fail(
                ResultCodes.Transaction.TransactionNotFound,
                "Tenant not found."));

        var result = await _sut.CreateDonationAsync(
            tenantPublicId,
            userPublicId,
            "Donation",
            "desc",
            100,
            "NZD",
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionCreateFailed);
        result.Message.Should().Be("Tenant information could not be resolved.");

        _transactionRepoMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task
        CreateDonationAsync_Should_Create_Transaction_And_Create_Tenant_Account_When_Payment_Succeeds_And_Account_Does_Not_Exist()
    {
        var tenantPublicId = Guid.NewGuid().ToString();
        var userPublicId = Guid.NewGuid().ToString();

        _tenantInfoClientMock
            .Setup(x => x.GetTenantSummaryAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantSummaryDto>.Ok(
                new TenantSummaryDto
                {
                    TenantPublicId = tenantPublicId,
                    TenantName = "Demo School"
                },
                ResultCodes.Transaction.TransactionQuerySuccess,
                "ok"));

        _paymentGatewayMock
            .Setup(x => x.PayAsync(It.IsAny<PaymentExecutionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentExecutionResult
            {
                Success = true,
                PaymentReference = "MOCK-PAY-001"
            });

        _tenantAccountRepoMock
            .Setup(x => x.GetByTenantPublicIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantAccount?)null);

        Transaction? capturedTransaction = null;
        _transactionRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((t, _) => capturedTransaction = t)
            .Returns(Task.CompletedTask);

        TenantAccount? capturedAccount = null;
        _tenantAccountRepoMock
            .Setup(x => x.AddAsync(It.IsAny<TenantAccount>(), It.IsAny<CancellationToken>()))
            .Callback<TenantAccount, CancellationToken>((a, _) => capturedAccount = a)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateDonationAsync(
            tenantPublicId,
            userPublicId,
            "Donation",
            "Monthly support",
            100,
            "NZD",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionCreateSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.TenantName.Should().Be("Demo School");
        result.Data.Type.Should().Be("Donation");
        result.Data.Status.Should().Be(TransactionStatus.Completed.ToString());
        result.Data.PaymentStatus.Should().Be(PaymentStatus.Succeeded.ToString());
        result.Data.PaymentReference.Should().Be("MOCK-PAY-001");

        capturedTransaction.Should().NotBeNull();
        capturedTransaction!.TenantNameSnapshot.Should().Be("Demo School");
        capturedTransaction.Type.Should().Be(TransactionType.Donation);
        capturedTransaction.Amount.Should().Be(100);
        capturedTransaction.PaymentStatus.Should().Be(PaymentStatus.Succeeded);
        capturedTransaction.Status.Should().Be(TransactionStatus.Completed);

        capturedAccount.Should().NotBeNull();
        capturedAccount!.AvailableBalance.Should().Be(100);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task CreateDonationAsync_Should_Increase_Existing_Tenant_Account_When_Payment_Succeeds()
    {
        var tenantGuid = Guid.NewGuid();
        var tenantPublicId = tenantGuid.ToString();
        var userPublicId = Guid.NewGuid().ToString();

        var existingAccount = new TenantAccount
        {
            TenantPublicId = tenantGuid,
            AvailableBalance = 250
        };

        _tenantInfoClientMock
            .Setup(x => x.GetTenantSummaryAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantSummaryDto>.Ok(
                new TenantSummaryDto
                {
                    TenantPublicId = tenantPublicId,
                    TenantName = "Demo School"
                },
                ResultCodes.Transaction.TransactionQuerySuccess,
                "ok"));

        _paymentGatewayMock
            .Setup(x => x.PayAsync(It.IsAny<PaymentExecutionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentExecutionResult
            {
                Success = true,
                PaymentReference = "MOCK-PAY-002"
            });

        _tenantAccountRepoMock
            .Setup(x => x.GetByTenantPublicIdAsync(tenantGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var result = await _sut.CreateDonationAsync(
            tenantPublicId,
            userPublicId,
            "Donation",
            null,
            50,
            "NZD",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        existingAccount.AvailableBalance.Should().Be(300);
        existingAccount.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _tenantAccountRepoMock.Verify(
            x => x.AddAsync(It.IsAny<TenantAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateDonationAsync_Should_Return_Success_But_Failed_Payment_Status_When_Payment_Fails()
    {
        var tenantPublicId = Guid.NewGuid().ToString();
        var userPublicId = Guid.NewGuid().ToString();

        _tenantInfoClientMock
            .Setup(x => x.GetTenantSummaryAsync(tenantPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ServiceResult<TenantSummaryDto>.Ok(
                new TenantSummaryDto
                {
                    TenantPublicId = tenantPublicId,
                    TenantName = "Demo School"
                },
                ResultCodes.Transaction.TransactionQuerySuccess,
                "ok"));

        _paymentGatewayMock
            .Setup(x => x.PayAsync(It.IsAny<PaymentExecutionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentExecutionResult
            {
                Success = false,
                FailureReason = "Mock payment rejected."
            });

        _tenantAccountRepoMock
            .Setup(x => x.GetByTenantPublicIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantAccount?)null);

        var result = await _sut.CreateDonationAsync(
            tenantPublicId,
            userPublicId,
            "Donation",
            null,
            100,
            "NZD",
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be(TransactionStatus.Failed.ToString());
        result.Data.PaymentStatus.Should().Be(PaymentStatus.Failed.ToString());
        result.Data.FailureReason.Should().Be("Mock payment rejected.");

        _tenantAccountRepoMock.Verify(
            x => x.AddAsync(It.IsAny<TenantAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyTransactionsAsync_Should_Return_Fail_When_Tenant_Is_Invalid()
    {
        var result = await _sut.GetMyTransactionsAsync(
            tenantPublicId: "invalid-tenant",
            userPublicId: Guid.NewGuid().ToString(),
            pageNumber: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("Tenant is invalid.");

        _transactionRepoMock.Verify(
            x => x.GetMyTransactionsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyTransactionsAsync_Should_Return_Fail_When_User_Is_Invalid()
    {
        var result = await _sut.GetMyTransactionsAsync(
            tenantPublicId: Guid.NewGuid().ToString(),
            userPublicId: "invalid-user",
            pageNumber: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("User is invalid.");

        _transactionRepoMock.Verify(
            x => x.GetMyTransactionsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetMyTransactionsAsync_Should_Normalize_PageNumber_And_PageSize()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _transactionRepoMock
            .Setup(x => x.GetMyTransactionsAsync(
                tenantId,
                userId,
                1,
                100,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Transaction>(), 0));

        var result = await _sut.GetMyTransactionsAsync(
            tenantPublicId: tenantId.ToString(),
            userPublicId: userId.ToString(),
            pageNumber: 0,
            pageSize: 999,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(100);

        _transactionRepoMock.Verify(
            x => x.GetMyTransactionsAsync(
                tenantId,
                userId,
                1,
                100,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMyTransactionsAsync_Should_Return_Paged_Result_When_Request_Is_Valid()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new List<Transaction>
        {
            new()
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantId,
                TenantNameSnapshot = "Demo School",
                CreatedByUserPublicId = userId,
                Type = TransactionType.Donation,
                Title = "Support Donation",
                Amount = 100,
                Currency = "NZD",
                Status = TransactionStatus.Completed,
                PaymentStatus = PaymentStatus.Succeeded,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
            },
            new()
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantId,
                TenantNameSnapshot = "Demo School",
                CreatedByUserPublicId = userId,
                Type = TransactionType.Procurement,
                Title = "Buy Stationery",
                Amount = 50,
                Currency = "NZD",
                Status = TransactionStatus.Submitted,
                PaymentStatus = PaymentStatus.NotStarted,
                RiskStatus = RiskStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        _transactionRepoMock
            .Setup(x => x.GetMyTransactionsAsync(
                tenantId,
                userId,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((transactions, 2));

        var result = await _sut.GetMyTransactionsAsync(
            tenantPublicId: tenantId.ToString(),
            userPublicId: userId.ToString(),
            pageNumber: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryByPageSuccess);
        result.Message.Should().Be("Transactions retrieved successfully.");

        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(2);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
        result.Data.Items.Should().HaveCount(2);

        result.Data.Items[0].TransactionPublicId.Should().Be(transactions[0].PublicId.ToString());
        result.Data.Items[0].TenantPublicId.Should().Be(tenantId.ToString());
        result.Data.Items[0].TenantName.Should().Be("Demo School");
        result.Data.Items[0].Type.Should().Be("Donation");
        result.Data.Items[0].Title.Should().Be("Support Donation");
        result.Data.Items[0].Amount.Should().Be(100);
        result.Data.Items[0].Currency.Should().Be("NZD");
        result.Data.Items[0].Status.Should().Be("Completed");
        result.Data.Items[0].PaymentStatus.Should().Be("Succeeded");
        result.Data.Items[0].RiskStatus.Should().Be("NotChecked");

        result.Data.Items[1].Type.Should().Be("Procurement");
        result.Data.Items[1].Title.Should().Be("Buy Stationery");
        result.Data.Items[1].PaymentStatus.Should().Be("NotStarted");
        result.Data.Items[1].RiskStatus.Should().Be("Pending");
    }

    [Fact]
    public async Task GetMyTransactionsAsync_Should_Return_Fail_When_Repository_Throws()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _transactionRepoMock
            .Setup(x => x.GetMyTransactionsAsync(
                tenantId,
                userId,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.GetMyTransactionsAsync(
            tenantPublicId: tenantId.ToString(),
            userPublicId: userId.ToString(),
            pageNumber: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("Failed to query transactions.");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Fail_When_Tenant_Is_Invalid()
    {
        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: "invalid-tenant",
            userPublicId: Guid.NewGuid().ToString(),
            role: "Member",
            transactionPublicId: Guid.NewGuid().ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("Tenant is invalid.");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Fail_When_User_Is_Invalid()
    {
        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: Guid.NewGuid().ToString(),
            userPublicId: "invalid-user",
            role: "Member",
            transactionPublicId: Guid.NewGuid().ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("User is invalid.");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Fail_When_TransactionPublicId_Is_Invalid()
    {
        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: Guid.NewGuid().ToString(),
            userPublicId: Guid.NewGuid().ToString(),
            role: "Member",
            transactionPublicId: "invalid-id",
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("Transaction public id is invalid.");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_NotFound_When_Transaction_Does_Not_Exist()
    {
        _transactionRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: Guid.NewGuid().ToString(),
            userPublicId: Guid.NewGuid().ToString(),
            role: "Member",
            transactionPublicId: Guid.NewGuid().ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionNotFound);
        result.Message.Should().Be("Transaction not found.");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_NotFound_When_Transaction_Belongs_To_Another_Tenant()
    {
        var currentTenantId = Guid.NewGuid();
        var anotherTenantId = Guid.NewGuid();

        var transaction = new Transaction
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = anotherTenantId,
            TenantNameSnapshot = "Another Tenant",
            CreatedByUserPublicId = Guid.NewGuid(),
            Type = TransactionType.Donation,
            Title = "Donation",
            Amount = 100,
            Currency = "NZD",
            Status = TransactionStatus.Completed,
            PaymentStatus = PaymentStatus.Succeeded,
            RiskStatus = RiskStatus.NotChecked,
            CreatedAtUtc = DateTime.UtcNow
        };

        _transactionRepoMock
            .Setup(x => x.GetByPublicIdAsync(transaction.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: currentTenantId.ToString(),
            userPublicId: Guid.NewGuid().ToString(),
            role: "Member",
            transactionPublicId: transaction.PublicId.ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionNotFound);
        result.Message.Should().Be("Transaction not found.");
    }

    [Fact]
    public async Task
        GetTransactionDetailAsync_Should_Return_Fail_When_Transaction_Does_Not_Belong_To_Current_User_And_User_Is_Not_Admin()
    {
        var tenantId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        var transaction = new Transaction
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantId,
            TenantNameSnapshot = "Demo Tenant",
            CreatedByUserPublicId = ownerUserId,
            Type = TransactionType.Donation,
            Title = "Donation",
            Amount = 100,
            Currency = "NZD",
            Status = TransactionStatus.Completed,
            PaymentStatus = PaymentStatus.Succeeded,
            RiskStatus = RiskStatus.NotChecked,
            CreatedAtUtc = DateTime.UtcNow
        };

        _transactionRepoMock
            .Setup(x => x.GetByPublicIdAsync(transaction.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: tenantId.ToString(),
            userPublicId: currentUserId.ToString(),
            role: "Member",
            transactionPublicId: transaction.PublicId.ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionNotBelongToCurrentUser);
        result.Message.Should().Be("Transaction does not belong to current user.");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Success_When_Current_User_Is_Owner()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transaction = new Transaction
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantId,
            TenantNameSnapshot = "Demo Tenant",
            CreatedByUserPublicId = userId,
            Type = TransactionType.Donation,
            Title = "Support Donation",
            Description = "Monthly support",
            Amount = 100,
            Currency = "NZD",
            Status = TransactionStatus.Completed,
            PaymentStatus = PaymentStatus.Succeeded,
            RiskStatus = RiskStatus.NotChecked,
            CreatedAtUtc = DateTime.UtcNow,
            PaymentReference = "MOCK-PAY-001"
        };

        _transactionRepoMock
            .Setup(x => x.GetByPublicIdAsync(transaction.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: tenantId.ToString(),
            userPublicId: userId.ToString(),
            role: "Member",
            transactionPublicId: transaction.PublicId.ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQuerySuccess);
        result.Data.Should().NotBeNull();
        result.Data!.TransactionPublicId.Should().Be(transaction.PublicId.ToString());
        result.Data.TenantPublicId.Should().Be(tenantId.ToString());
        result.Data.TenantName.Should().Be("Demo Tenant");
        result.Data.Type.Should().Be("Donation");
        result.Data.Title.Should().Be("Support Donation");
        result.Data.Description.Should().Be("Monthly support");
        result.Data.Amount.Should().Be(100);
        result.Data.Currency.Should().Be("NZD");
        result.Data.Status.Should().Be("Completed");
        result.Data.PaymentStatus.Should().Be("Succeeded");
        result.Data.PaymentReference.Should().Be("MOCK-PAY-001");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Success_When_Current_User_Is_Admin()
    {
        var tenantId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();

        var transaction = new Transaction
        {
            PublicId = Guid.NewGuid(),
            TenantPublicId = tenantId,
            TenantNameSnapshot = "Demo Tenant",
            CreatedByUserPublicId = ownerUserId,
            Type = TransactionType.Procurement,
            Title = "Buy Stationery",
            Amount = 50,
            Currency = "NZD",
            Status = TransactionStatus.Submitted,
            PaymentStatus = PaymentStatus.NotStarted,
            RiskStatus = RiskStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        _transactionRepoMock
            .Setup(x => x.GetByPublicIdAsync(transaction.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: tenantId.ToString(),
            userPublicId: adminUserId.ToString(),
            role: "Admin",
            transactionPublicId: transaction.PublicId.ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Type.Should().Be("Procurement");
        result.Data.Title.Should().Be("Buy Stationery");
    }

    [Fact]
    public async Task GetTransactionDetailAsync_Should_Return_Fail_When_Repository_Throws()
    {
        _transactionRepoMock
            .Setup(x => x.GetByPublicIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db error"));

        var result = await _sut.GetTransactionDetailAsync(
            tenantPublicId: Guid.NewGuid().ToString(),
            userPublicId: Guid.NewGuid().ToString(),
            role: "Member",
            transactionPublicId: Guid.NewGuid().ToString(),
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("Failed to query transaction.");
    }

    [Fact]
    public async Task GetTransactionsAsync_Should_Return_Fail_When_Tenant_Is_Invalid()
    {
        var result = await _sut.GetTransactionsAsync(
            tenantPublicId: "invalid-tenant",
            role: "Admin",
            type: null,
            status: null,
            paymentStatus: null,
            pageNumber: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryFailed);
        result.Message.Should().Be("Tenant is invalid.");
    }

    [Fact]
    public async Task GetTransactionsAsync_Should_Return_Fail_When_User_Is_Not_Admin()
    {
        var result = await _sut.GetTransactionsAsync(
            tenantPublicId: Guid.NewGuid().ToString(),
            role: "Member",
            type: null,
            status: null,
            paymentStatus: null,
            pageNumber: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionNotBelongToCurrentUser);
        result.Message.Should().Be("Only admin can query tenant transactions.");
    }

    [Fact]
    public async Task GetTransactionsAsync_Should_Return_Paged_Result_When_Request_Is_Valid()
    {
        var tenantId = Guid.NewGuid();

        var transactions = new List<Transaction>
        {
            new()
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantId,
                TenantNameSnapshot = "Demo Tenant",
                CreatedByUserPublicId = Guid.NewGuid(),
                Type = TransactionType.Donation,
                Title = "Donation A",
                Amount = 100,
                Currency = "NZD",
                Status = TransactionStatus.Completed,
                PaymentStatus = PaymentStatus.Succeeded,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
            },
            new()
            {
                PublicId = Guid.NewGuid(),
                TenantPublicId = tenantId,
                TenantNameSnapshot = "Demo Tenant",
                CreatedByUserPublicId = Guid.NewGuid(),
                Type = TransactionType.Procurement,
                Title = "Procurement A",
                Amount = 50,
                Currency = "NZD",
                Status = TransactionStatus.Submitted,
                PaymentStatus = PaymentStatus.NotStarted,
                RiskStatus = RiskStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        _transactionRepoMock
            .Setup(x => x.GetTransactionsAsync(
                tenantId,
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((transactions, 2));

        var result = await _sut.GetTransactionsAsync(
            tenantPublicId: tenantId.ToString(),
            role: "Admin",
            type: null,
            status: null,
            paymentStatus: null,
            pageNumber: 1,
            pageSize: 10,
            cancellationToken: CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Code.Should().Be(ResultCodes.Transaction.TransactionQueryByPageSuccess);
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}