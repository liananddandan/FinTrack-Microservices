using System.Net;

namespace SharedKernel.Common.Exceptions;

public class UserNotFoundException : BaseAppException
{
    public UserNotFoundException(string message) 
        : base(message, code: "User Not Found", statusCode: (int)HttpStatusCode.NotFound)
    {
    }
}