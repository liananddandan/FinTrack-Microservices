namespace SharedKernel.Common.Exceptions;

public class BaseAppException : Exception
{
    public string Code { get; }
    public int? StatusCode { get; }

    public BaseAppException(string message, string code, int? statusCode = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}