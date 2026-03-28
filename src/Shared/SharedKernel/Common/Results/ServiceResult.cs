namespace SharedKernel.Common.Results;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    
    public static ServiceResult<T> Ok(T data, string code, string? message = null)
        => new() {Success = true, Code = code, Message = message, Data = data};
    
    public static ServiceResult<T> Fail(string code , string message)
        => new() {Success = false, Code = code, Message = message};
}