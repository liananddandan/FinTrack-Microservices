using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityService.Tests.IntegrationTests;

[Collection("IntegrationTests")]
public class AccountRegisterTests : IClassFixture<IdentityWebApplicationFactory<Program>>
{
    private readonly IdentityWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccountRegisterTests(IdentityWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterUser_Should_Return_BadRequest_When_UserName_Is_Empty()
    {
        var unique = Guid.NewGuid().ToString("N");

        var request = new
        {
            userName = "",
            email = $"user-{unique}@test.com",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/account/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, body);
        body.Should().Contain("User name is required.");
    }

    [Fact]
    public async Task RegisterUser_Should_Return_Ok_When_Request_Is_Valid()
    {
        var unique = Guid.NewGuid().ToString("N");

        var request = new
        {
            userName = "Emily",
            email = $"user-{unique}@test.com",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/account/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterUserResultTestDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();

        apiResponse.Data!.Email.Should().Be(request.email);
        apiResponse.Data.UserName.Should().Be(request.userName);
        apiResponse.Data.UserPublicId.Should().NotBeNullOrWhiteSpace();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var user = db.Users.FirstOrDefault(x => x.Email == request.email);
        user.Should().NotBeNull();
        user!.UserName.Should().Be(request.userName);
    }

    [Fact]
    public async Task RegisterUser_Should_Return_BadRequest_When_Email_Already_Exists()
    {
        var unique = Guid.NewGuid().ToString("N");
        var email = $"duplicate-{unique}@test.com";

        var firstRequest = new
        {
            userName = $"Emily-{unique}",
            email,
            password = "Password123!"
        };

        var secondRequest = new
        {
            userName = $"AnotherUser-{unique}",
            email,
            password = "Password123!"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/account/register", firstRequest);
        var firstBody = await firstResponse.Content.ReadAsStringAsync();
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK, firstBody);

        var secondResponse = await _client.PostAsJsonAsync("/api/account/register", secondRequest);
        var secondBody = await secondResponse.Content.ReadAsStringAsync();

        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, secondBody);
        secondBody.Should().Contain("Email already exists.");
    }

    [Fact]
    public async Task RegisterUser_Should_Store_Email_In_Lowercase()
    {
        var unique = Guid.NewGuid().ToString("N");

        var request = new
        {
            userName = "ChenLi",
            email = $"ChenLi-{unique}@Example.com",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/account/register", request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterUserResultTestDto>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().NotBeNull();

        apiResponse.Data!.Email.Should().Be(request.email.ToLowerInvariant());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        var user = db.Users.FirstOrDefault(x => x.Email == request.email.ToLowerInvariant());
        user.Should().NotBeNull();
    }

    private sealed class ApiResponse<T>
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class RegisterUserResultTestDto
    {
        public string UserPublicId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}