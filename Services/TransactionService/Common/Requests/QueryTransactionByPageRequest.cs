namespace TransactionService.Common.Requests;

public record QueryTransactionByPageRequest(
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 10,
    string SortBy = "desc"
);