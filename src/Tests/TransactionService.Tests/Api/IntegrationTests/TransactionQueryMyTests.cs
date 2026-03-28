using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Common.DTOs;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence;
using Xunit;

namespace TransactionService.Tests.Api.IntegrationTests;

[Collection("NonParallel Collection")]
public class TransactionGetMyTests : IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TransactionGetMyTests(TransactionWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMyTransactions_Should_Return_Only_Current_User_Transactions()
    {
        var tenantPublicId = Guid.NewGuid();
        var currentUserPublicId = Guid.NewGuid();
        var otherUserPublicId = Guid.NewGuid();

        await SeedTransactionsAsync(tenantPublicId, currentUserPublicId, otherUserPublicId);
        
        _client.SetTestAuth("Member", currentUserPublicId, tenantPublicId);

        var response = await _client.GetAsync("/api/transactions/my?pageNumber=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionListItemDto>>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.TotalCount.Should().Be(2);
        apiResponse.Data.Items.Should().HaveCount(2);
    }

    private async Task SeedTransactionsAsync(
        Guid tenantPublicId,
        Guid currentUserPublicId,
        Guid otherUserPublicId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        db.Transactions.AddRange(
            new Transaction
            {
                TenantPublicId = tenantPublicId,
                TenantNameSnapshot = "Demo Tenant",
                CreatedByUserPublicId = currentUserPublicId,
                Type = TransactionType.Donation,
                Title = "Donation A",
                Amount = 100,
                Currency = "NZD",
                Status = TransactionStatus.Completed,
                PaymentStatus = PaymentStatus.Succeeded,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
            },
            new Transaction
            {
                TenantPublicId = tenantPublicId,
                TenantNameSnapshot = "Demo Tenant",
                CreatedByUserPublicId = currentUserPublicId,
                Type = TransactionType.Donation,
                Title = "Donation B",
                Amount = 200,
                Currency = "NZD",
                Status = TransactionStatus.Completed,
                PaymentStatus = PaymentStatus.Succeeded,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
            },
            new Transaction
            {
                TenantPublicId = tenantPublicId,
                TenantNameSnapshot = "Demo Tenant",
                CreatedByUserPublicId = otherUserPublicId,
                Type = TransactionType.Donation,
                Title = "Other User Donation",
                Amount = 300,
                Currency = "NZD",
                Status = TransactionStatus.Completed,
                PaymentStatus = PaymentStatus.Succeeded,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1)
            });

        await db.SaveChangesAsync();
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}