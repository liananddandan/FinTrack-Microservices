namespace TransactionService.Application.Orders.Dtos;

public class OrderSummaryDto
{
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int CancelledOrderCount { get; set; }
}