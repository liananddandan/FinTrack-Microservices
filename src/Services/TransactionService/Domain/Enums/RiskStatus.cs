namespace TransactionService.Domain.Enums;

public enum RiskStatus
{
    NotChecked = 1,
    Pending = 2,
    Passed = 3,
    Flagged = 4,
    Blocked = 5
}