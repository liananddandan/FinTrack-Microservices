namespace IdentityService.Application.Common.Abstractions;

public interface IBootstrapAdminService
{
    Task EnsureBootstrapSuperAdminAsync(CancellationToken cancellationToken = default);
}