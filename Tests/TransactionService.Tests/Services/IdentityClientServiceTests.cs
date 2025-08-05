using FluentAssertions;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.ExternalServices;
using TransactionService.Services;
using TransactionService.Tests.Attributes;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace TransactionService.Tests.Services;

public class IdentityClientServiceTests : IAsyncLifetime
{
    private WireMockServer _wireMockServer;
    private IdentityClientService _identityClientService;
    
    [Theory, AutoMoqData]
    public async Task GetUserInfoAsync_ShouldReturnNull_WhenResponseNotSuccess(
        string userPublicId)
    {
        // arrange
        _wireMockServer.Given(Request.Create().WithPath($"/api/internal/account/{userPublicId}")
            .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));
        
        // act
        var result = await _identityClientService.GetUserInfoAsync(userPublicId);
        
        // assert
        result.Should().BeNull();
    }
    
    [Theory, AutoMoqData]
    public async Task GetUserInfoAsync_ShouldReturnUserInfo_WhenResponseSuccess(
        string userPublicId,
        UserInfoDto userInfo)
    {
        // arrange
        _wireMockServer.Given(Request.Create().WithPath($"/api/internal/account/{userPublicId}")
                .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBodyAsJson(ServiceResult<UserInfoDto>
                    .Ok(userInfo, "return userinfo", "return userinfo")));
        
        // act
        var result = await _identityClientService.GetUserInfoAsync(userPublicId);
        
        // assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(userInfo);
    }

    public Task InitializeAsync()
    {
        _wireMockServer = WireMockServer.Start(8899); 
        var httpClient = new HttpClient() {BaseAddress = new Uri(_wireMockServer.Urls[0])};
        _identityClientService = new IdentityClientService(httpClient);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _wireMockServer.Stop();
        _wireMockServer.Dispose();
        return Task.CompletedTask;
    }
}