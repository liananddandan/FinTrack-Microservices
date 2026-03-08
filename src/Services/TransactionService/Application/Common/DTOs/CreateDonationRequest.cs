namespace TransactionService.Application.Common.DTOs;

public class CreateDonationRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NZD";
}