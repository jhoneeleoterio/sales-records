using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed record ListSalesResult(
    IReadOnlyCollection<ListSalesItemResult> Sales,
    int TotalCount);

public sealed record ListSalesItemResult(
    Guid Id,
    int Number,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    decimal TotalAmount,
    SaleStatus Status,
    DateTime CreatedAt);
