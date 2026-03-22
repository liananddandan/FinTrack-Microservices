namespace GatewayService.Application.Common.Options;

public class OpenApiServicesOptions
{
    public string Identity { get; set; } = default!;
    public string Transaction { get; set; } = default!;
    public string AuditLog { get; set; } = default!;
}