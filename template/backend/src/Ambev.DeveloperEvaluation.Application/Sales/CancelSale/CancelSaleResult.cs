using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public sealed record CancelSaleResult(
    Guid Id,
    SaleStatus Status,
    DateTime? UpdatedAt);
