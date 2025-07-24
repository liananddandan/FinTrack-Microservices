using System.Net.Http.Json;
using FluentAssertions;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Common.Requests;
using TransactionService.Tests.Attributes;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit.Abstractions;

namespace TransactionService.Tests.Controllers;

public class TransactionControllerTests(ITestOutputHelper testOutputHelper) : IAsyncLifetime
{
    private WireMockServer _wireMockServer;
    private TransactionWebApplicationFactory<Program> _mockFactory;
    private HttpClient _client;
    
    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start(9999);
        _mockFactory = new TransactionWebApplicationFactory<Program>(_wireMockServer.Urls[0]);
        _client = _mockFactory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer.Stop();
        _wireMockServer.Dispose();
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Theory, AutoMoqData]
    public async Task CreateTransactionAsync_ShouldReturnOk_WhenEverythingIsOk(
        Guid userPublicId,
        Guid tenantPublicId,
        long jwtVersion,
        string roleInTenant,
        CreateTransactionRequest request,
        UserInfoDto userInfo)
    {
        // Arrange
        userInfo.tenantInfoDto.TenantPublicId = tenantPublicId.ToString();
        var token = JwtTokenGenerator.GenerateFakeAccessToken(userPublicId.ToString(),
            jwtVersion.ToString(), tenantPublicId.ToString(), roleInTenant);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        _wireMockServer.Given(Request.Create().WithPath($"/api/internal/account/{userPublicId}")
                .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(userInfo));
        
        // Act
        var response = await _client.PostAsJsonAsync("api/transaction/create", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldQueryResult()
    {
        var transactionPublicId = "11111111-1111-1111-1111-111111111111";
        var tenantPublicId = "11111111-1111-1111-1111-111111111111";
        var userPublicId = "11111111-1111-1111-1111-111111111111";
        var token = JwtTokenGenerator.GenerateFakeAccessToken(userPublicId,
            "4", tenantPublicId, "User_test");
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        // act
        var response = await _client.GetAsync($"api/transaction/{transactionPublicId}");
        
        // assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine(content);
        content.Should().Contain(ResultCodes.Transaction.TransactionQuerySuccess);
    }
}