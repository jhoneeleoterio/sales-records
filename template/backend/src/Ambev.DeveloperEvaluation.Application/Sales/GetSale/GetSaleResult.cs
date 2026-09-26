using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public sealed record GetSaleResult(
    Guid Id,
    int Number,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    int Quantity,
    decimal TotalAmount,
    decimal Discount,
    SaleStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyCollection<GetSaleItemResult> Items);

public sealed record GetSaleItemResult(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Discount,
    decimal TotalAmount);
