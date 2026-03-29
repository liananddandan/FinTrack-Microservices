using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence;

namespace TransactionService.Tests.Integration.Transactions;

[Collection("NonParallel Collection")]
public class TransactionQueryTenantTests(TransactionWebApplicationFactory<Program> factory)
    : IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetTransactions_Should_Return_Unauthorized_When_No_Token()
    {
        await factory.ResetDatabaseAsync();
        ClearTestAuth();

        var response = await _client.GetAsync("/api/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactions_Should_Return_BadRequest_When_User_Is_Not_Admin()
    {
        await factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();

        _client.SetTestAuth(
            role: "Member",
            userPublicId: Guid.NewGuid(),
            tenantPublicId: tenantId);

        var response = await _client.GetAsync("/api/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactions_Should_Return_Tenant_Transactions_For_Admin()
    {
        await factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
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

        _client.SetTestAuth(
            role: "Admin",
            userPublicId: Guid.NewGuid(),
            tenantPublicId: tenantId);

        var response = await _client.GetAsync("/api/transactions?pageNumber=1&pageSize=10");
        var raw = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, raw);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<TransactionListItemDto>>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().HaveCount(2);
        apiResponse.Data.TotalCount.Should().Be(2);
        apiResponse.Data.Items.Should().OnlyContain(x => x.TenantPublicId == tenantId.ToString());
    }

    private void ClearTestAuth()
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.TenantIdHeader);
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}