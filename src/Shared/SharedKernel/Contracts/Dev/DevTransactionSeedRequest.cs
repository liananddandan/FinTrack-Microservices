namespace SharedKernel.Contracts.Dev;

public record DevTransactionSeedRequest
{
    public required string TenantPublicId { get; init; }
    public required string TenantName { get; init; }
    public required string AdminUserPublicId { get; init; }
    public required string MemberUserPublicId { get; init; }
    public required string Template { get; init; }
}
