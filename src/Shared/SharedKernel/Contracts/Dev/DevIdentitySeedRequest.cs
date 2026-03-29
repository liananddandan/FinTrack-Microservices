namespace SharedKernel.Contracts.Dev;

public record DevIdentitySeedRequest
{
    public required string TenantName { get; init; }
    public required string AdminEmail { get; init; }
    public required string AdminPassword { get; init; }
    public required string MemberEmail { get; init; }
    public required string MemberPassword { get; init; }
}