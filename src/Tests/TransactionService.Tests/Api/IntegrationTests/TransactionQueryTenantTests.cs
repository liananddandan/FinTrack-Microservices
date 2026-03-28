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

[Collection("IntegrationTests")]
public class TransactionQueryTenantTests : IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TransactionQueryTenantTests(TransactionWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactions_Should_Return_Unauthorized_When_No_Token()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("/api/transactions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactions_Should_Return_BadRequest_When_User_Is_Not_Admin()
    {
        await _factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();
        var token = JwtTestTokenFactory.CreateTenantAccessToken(
            userPublicId: Guid.NewGuid().ToString(),
            tenantPublicId: tenantId.ToString(),
            role: "Member");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/transactions");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactions_Should_Return_Tenant_Transactions_For_Admin()
    {
        await _factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

            db.Transactions.AddRange(
                new Transaction
                {
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
                new Transaction
                {
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
                },
                new Transaction
                {
                    TenantPublicId = otherTenantId,
                    TenantNameSnapshot = "Other Tenant",
                    CreatedByUserPublicId = Guid.NewGuid(),
                    Type = TransactionType.Donation,
                    Title = "Other Tenant Donation",
                    Amount = 999,
                    Currency = "NZD",
                    Status = TransactionStatus.Completed,
                    PaymentStatus = PaymentStatus.Succeeded,
                    RiskStatus = RiskStatus.NotChecked,
                    CreatedAtUtc = DateTime.UtcNow
                });

            await db.SaveChangesAsync();
        }

        var token = JwtTestTokenFactory.CreateTenantAccessToken(
            userPublicId: Guid.NewGuid().ToString(),
            tenantPublicId: tenantId.ToString(),
            role: "Admin");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/transactions?pageNumber=1&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionListItemDto>>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(2);
        apiResponse.Data.TotalCount.Should().Be(2);
        apiResponse.Data.Items.Should().OnlyContain(x => x.TenantPublicId == tenantId.ToString());
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}