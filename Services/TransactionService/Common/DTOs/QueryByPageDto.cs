namespace TransactionService.Common.DTOs;

public record QueryByPageDto(List<TransactionDto> transactions, int totalCount, int page, int pageSize);