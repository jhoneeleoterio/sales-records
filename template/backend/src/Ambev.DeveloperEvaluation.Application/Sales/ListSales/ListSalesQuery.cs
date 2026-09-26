using MediatR;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed record ListSalesQuery(
    int PageNumber,
    int PageSize,
    int? Number = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    Guid? ProductId = null,
    SaleStatus? Status = null,
    decimal? MinTotalAmount = null,
    decimal? MaxTotalAmount = null,
    DateTime? MinCreatedAt = null,
    DateTime? MaxCreatedAt = null,
    string? Order = null) : IRequest<ListSalesResult>;
