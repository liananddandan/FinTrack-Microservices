using Microsoft.AspNetCore.Mvc;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;

namespace IdentityService.Common.Extensions;

public static class ServiceResultExtensions
{
    public static IActionResult ToActionResult<T>(this ServiceResult<T> serviceResult)
    {
        var response = new ApiResponse<T>(
            serviceResult.Code ?? ResultCodes.InternalError,
            serviceResult.Message ?? "",
            serviceResult.Data
            );
        return serviceResult.Success ? new OkObjectResult(response) : new BadRequestObjectResult(response);
    }
}