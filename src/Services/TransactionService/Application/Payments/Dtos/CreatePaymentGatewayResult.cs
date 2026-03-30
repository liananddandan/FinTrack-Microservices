namespace TransactionService.Application.Payments.Dtos;

public class CreatePaymentGatewayResult
{
    public required string ProviderPaymentReference { get; set; }
    public string? ClientSecret { get; set; }
    public required string Status { get; set; }
}