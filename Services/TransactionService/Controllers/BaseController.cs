using Microsoft.AspNetCore.Mvc;

namespace TransactionService.Controllers;

public class BaseController : ControllerBase
{
    protected string TenantPublicId => User.FindFirst("TenantPublicId")!.Value;
    protected string UserPublicId => User.FindFirst("UserPublicId")!.Value;
    protected string UserRoleInTenant => User.FindFirst("UserRoleInTenant")!.Value;
}