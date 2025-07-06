namespace SharedKernel.Common.Results;

public record ApiResponse<T>(
    string Code,
    string Message,
    T? Data
    );