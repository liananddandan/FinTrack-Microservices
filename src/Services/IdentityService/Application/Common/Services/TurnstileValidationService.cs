using IdentityService.Application.Common.Abstractions;
using IdentityService.Application.Common.DTOs;
using IdentityService.Application.Common.Options;
using Microsoft.Extensions.Options;
using SharedKernel.Common.Results;

namespace IdentityService.Application.Common.Services;

public class TurnstileValidationService(
    HttpClient httpClient,
    IOptions<TurnstileOptions> options,
    ILogger<TurnstileValidationService> logger)
    : ITurnstileValidationService
{
    private readonly TurnstileOptions _options = options.Value;

    public async Task<ServiceResult<bool>> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ServiceResult<bool>.Fail(
                "TURNSTILE_TOKEN_REQUIRED",
                "Verification challenge is required.");
        }

        try
        {
            var payload = new Dictionary<string, string>
            {
                ["secret"] = _options.SecretKey,
                ["response"] = token
            };

            using var content = new FormUrlEncodedContent(payload);

            var response = await httpClient.PostAsync(
                _options.VerifyUrl,
                content,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogInformation(
                "Turnstile verify response. StatusCode: {StatusCode}, Body: {Body}",
                response.StatusCode,
                responseBody);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.Fail(
                    "TURNSTILE_VERIFY_FAILED",
                    "Verification failed. Please try again.");
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<TurnstileVerifyResponse>(
                responseBody,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result is null || !result.Success)
            {
                logger.LogWarning(
                    "Turnstile validation failed. Errors: {Errors}",
                    result is null ? "null response" : string.Join(", ", result.ErrorCodes));

                return ServiceResult<bool>.Fail(
                    "TURNSTILE_VERIFY_FAILED",
                    "Verification failed. Please try again.");
            }

            return ServiceResult<bool>.Ok(
                true,
                "TURNSTILE_VERIFY_SUCCESS",
                "Verification passed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Turnstile validation request failed.");

            return ServiceResult<bool>.Fail(
                "TURNSTILE_VERIFY_EXCEPTION",
                "Verification failed. Please try again.");
        }
    }
}