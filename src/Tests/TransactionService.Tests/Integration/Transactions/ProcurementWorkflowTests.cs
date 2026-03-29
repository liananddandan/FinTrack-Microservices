using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Api.Transaction.Contracts;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Infrastructure.Persistence;

namespace TransactionService.Tests.Integration.Transactions;

[Collection("NonParallel Collection")]
public class ProcurementWorkflowTests : IClassFixture<TransactionWebApplicationFactory<Program>>
{
    private readonly TransactionWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProcurementWorkflowTests(TransactionWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Update_Submit_Approve_Reject_Procurement_Workflow_Should_Work()
    {
        await _factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        
        _client.SetTestAuth("Member", userId, tenantId);

        // create
        var createResponse = await _client.PostAsJsonAsync("/api/transactions/procurements",
            new CreateProcurementRequest
            {
                Title = "Buy Laptop",
                Description = "Procurement draft",
                Amount = 2000,
                Currency = "NZD"
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createApi = await createResponse.Content.ReadFromJsonAsync<ApiResponse<CreateTransactionResult>>();
        createApi!.Data.Should().NotBeNull();

        var transactionId = createApi.Data!.TransactionPublicId;

        // update
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/transactions/procurements/{transactionId}",
            new UpdateProcurementRequest
            {
                Title = "Buy MacBook",
                Description = "Updated draft",
                Amount = 2500,
                Currency = "USD"
            });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // submit
        var submitResponse = await _client.PostAsync(
            $"/api/transactions/{transactionId}/submit",
            null);

        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // approve as admin
        _client.SetTestAuth("Admin", adminId, tenantId);

        var approveResponse = await _client.PostAsync(
            $"/api/transactions/{transactionId}/approve",
            null);

        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var transaction = await db.Transactions.FirstAsync(x => x.PublicId.ToString() == transactionId);

        transaction.Title.Should().Be("Buy MacBook");
        transaction.Amount.Should().Be(2500);
        transaction.Currency.Should().Be("USD");
        transaction.Status.Should().Be(TransactionStatus.Approved);
    }

    [Fact]
    public async Task Reject_Submitted_Procurement_Should_Work()
    {
        await _factory.ResetDatabaseAsync();

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

            db.Transactions.Add(new Transaction
            {
                TenantPublicId = tenantId,
                TenantNameSnapshot = "Demo Tenant",
                CreatedByUserPublicId = userId,
                Type = TransactionType.Procurement,
                Title = "Buy Mouse",
                Amount = 50,
                Currency = "NZD",
                Status = TransactionStatus.Submitted,
                PaymentStatus = PaymentStatus.NotStarted,
                RiskStatus = RiskStatus.NotChecked,
                CreatedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        var existing = await verifyDb.Transactions.FirstAsync();
        
        _client.SetTestAuth("Admin", adminId, tenantId);

        var rejectResponse = await _client.PostAsJsonAsync(
            $"/api/transactions/{existing.PublicId}/reject",
            new RejectProcurementRequest
            {
                Reason = "Budget exceeded"
            });

        rejectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<TransactionDbContext>();
        var updated = await assertDb.Transactions.FirstAsync(x => x.PublicId == existing.PublicId);
        updated.Status.Should().Be(TransactionStatus.Rejected);
        updated.FailureReason.Should().Be("Budget exceeded");
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}