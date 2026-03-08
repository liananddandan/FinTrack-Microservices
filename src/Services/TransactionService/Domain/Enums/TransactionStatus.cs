namespace TransactionService.Domain.Enums;


public enum TransactionStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Completed = 5,
    Failed = 6,
    Cancelled = 7
}