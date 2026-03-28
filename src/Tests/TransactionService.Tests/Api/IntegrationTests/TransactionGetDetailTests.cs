using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common.Results;
using TransactionService.Application.Common.DTOs;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence;

namespace TransactionService.Tests.Api.IntegrationTests;

[Collection("NonParallel Collection")]
public class TransactionGetDetailTests : IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TransactionGetDetailTests(TransactionWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactionDetail_Should_Return_Unauthorized_When_No_Token()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactionDetail_Should_Return_Detail_When_Current_User_Is_Owner()
    {
        await _factory.ResetDatabaseAsync();

        var tenantPublicId = Guid.NewGuid();
        var userPublicId = Guid.NewGuid();

        var transaction = await SeedTransactionAsync(
            tenantPublicId,
            userPublicId,
            "Support Donation");
        
        _client.SetTestAuth("Member", userPublicId, tenantPublicId);

        var response = await _client.GetAsync($"/api/transactions/{transaction.PublicId}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDetailDto>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.TransactionPublicId.Should().Be(transaction.PublicId.ToString());
        apiResponse.Data.Title.Should().Be("Support Donation");
        apiResponse.Data.Type.Should().Be("Donation");
    }

    [Fact]
    public async Task GetTransactionDetail_Should_Return_BadRequest_When_User_Is_Not_Owner_And_Not_Admin()
    {
        await _factory.ResetDatabaseAsync();

        var tenantPublicId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        var transaction = await SeedTransactionAsync(
            tenantPublicId,
            ownerUserId,
            "Owner Donation");
        
        _client.SetTestAuth("Member", currentUserId, tenantPublicId);

        var response = await _client.GetAsync($"/api/transactions/{transaction.PublicId}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain(ResultCodes.Transaction.TransactionNotBelongToCurrentUser);
    }

    [Fact]
    public async Task GetTransactionDetail_Should_Return_Detail_When_Current_User_Is_Admin()
    {
        await _factory.ResetDatabaseAsync();

        var tenantPublicId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var transaction = await SeedTransactionAsync(
            tenantPublicId,
            ownerUserId,
            "Admin Visible Donation");
        
        _client.SetTestAuth("Member", ownerUserId, tenantPublicId);

        var response = await _client.GetAsync($"/api/transactions/{transaction.PublicId}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDetailDto>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Title.Should().Be("Admin Visible Donation");
    }

    [Fact]
    public async Task GetTransactionDetail_Should_Return_NotFound_When_Transaction_Belongs_To_Another_Tenant()
    {
        await _factory.ResetDatabaseAsync();

        var ownerTenantPublicId = Guid.NewGuid();
        var currentTenantPublicId = Guid.NewGuid();
        var userPublicId = Guid.NewGuid();

        var transaction = await SeedTransactionAsync(
            ownerTenantPublicId,
            userPublicId,
            "Cross Tenant Donation");
        
        _client.SetTestAuth("Member", userPublicId, currentTenantPublicId);

        var response = await _client.GetAsync($"/api/transactions/{transaction.PublicId}");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain(ResultCodes.Transaction.TransactionNotFound);
    }

    private async Task<Transaction> SeedTransactionAsync(
        Guid tenantPublicId,
        Guid createdByUserPublicId,
        string title)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var transaction = new Transaction
        {
            TenantPublicId = tenantPublicId,
            TenantNameSnapshot = "Demo Tenant",
            CreatedByUserPublicId = createdByUserPublicId,
            Type = TransactionType.Donation,
            Title = title,
            Description = "Test description",
            Amount = 100,
            Currency = "NZD",
            Status = TransactionStatus.Completed,
            PaymentStatus = PaymentStatus.Succeeded,
            RiskStatus = RiskStatus.NotChecked,
            CreatedAtUtc = DateTime.UtcNow,
            PaymentReference = "MOCK-PAY-001"
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        return transaction;
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}