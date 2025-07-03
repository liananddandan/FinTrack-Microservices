namespace IdentityService.Common.Results;

public record ApiResponse<T>(
    string Code,
    string Message,
    T? Data
    );