namespace SharedKernel.Common.DTOs;

public record ApiResponse<T>(
    string Code,
    string Message,
    T? Data
    );