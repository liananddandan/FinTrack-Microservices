using System.Text.Json.Serialization;

namespace IdentityService.Application.Common.DTOs;

public class TurnstileVerifyResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error-codes")]
    public List<string> ErrorCodes { get; set; } = new();
}