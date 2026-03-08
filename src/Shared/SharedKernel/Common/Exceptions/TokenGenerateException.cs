using System.Net;

namespace SharedKernel.Common.Exceptions;

public class TokenGenerateException : BaseAppException
{
    public TokenGenerateException(string message): base(message, code: "TokenGenerateException", (int)HttpStatusCode.InternalServerError) { }
}