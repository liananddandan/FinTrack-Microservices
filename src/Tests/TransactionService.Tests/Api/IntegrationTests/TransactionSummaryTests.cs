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
public class TransactionSummaryTests : IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TransactionSummaryTests(TransactionWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactionSummary_Should_Return_Unauthorized_When_No_Token()
    {
        await _factory.ResetDatabaseAsync();

        var response = await _client.GetAsync("/api/transactions/summary");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactionSummary_Should_Return_BadRequest_When_User_Is_Not_Admin()
    {
        await _factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();
        var token = JwtTestTokenFactory.CreateTenantAccessToken(
            userPublicId: Guid.NewGuid().ToString(),
            tenantPublicId: tenantId.ToString(),
            role: "Member");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/transactions/summary");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactionSummary_Should_Return_Summary_For_Admin()
    {
        await _factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();

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
                    Amount = 200,
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
                    Status = TransactionStatus.Completed,
                    PaymentStatus = PaymentStatus.Succeeded,
                    RiskStatus = RiskStatus.NotChecked,
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
                },
                new Transaction
                {
                    TenantPublicId = tenantId,
                    TenantNameSnapshot = "Demo Tenant",
                    CreatedByUserPublicId = Guid.NewGuid(),
                    Type = TransactionType.Donation,
                    Title = "Donation B",
                    Amount = 100,
                    Currency = "NZD",
                    Status = TransactionStatus.Failed,
                    PaymentStatus = PaymentStatus.Failed,
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

        var response = await _client.GetAsync("/api/transactions/summary");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse =
            await response.Content.ReadFromJsonAsync<ApiResponse<TenantTransactionSummaryDto>>();

        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();
        apiResponse.Data!.TenantPublicId.Should().Be(tenantId.ToString());
        apiResponse.Data.TenantName.Should().Be("Demo Tenant");
        apiResponse.Data.TotalDonationAmount.Should().Be(200);
        apiResponse.Data.TotalProcurementAmount.Should().Be(50);
        apiResponse.Data.CurrentBalance.Should().Be(150);
        apiResponse.Data.TotalTransactionCount.Should().Be(3);
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}