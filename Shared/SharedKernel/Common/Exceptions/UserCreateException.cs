using System.Net;

namespace SharedKernel.Common.Exceptions;

public class UserCreateException : BaseAppException
{
    public UserCreateException(string message) : base(message, code: "UserCreateException", (int)HttpStatusCode.InternalServerError)
    {
    }
}