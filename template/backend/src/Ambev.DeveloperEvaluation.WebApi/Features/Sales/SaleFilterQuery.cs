using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

/// <summary>
/// Represents the query-string filters supported when listing sales.
/// </summary>
public sealed class SaleFilterQuery
{
    [FromQuery(Name = "_page")]
    public int PageNumber { get; init; } = 1;

    [FromQuery(Name = "_size")]
    public int PageSize { get; init; } = 10;

    [FromQuery]
    public int? Number { get; init; }

    [FromQuery]
    public Guid? CustomerId { get; init; }

    [FromQuery]
    public Guid? BranchId { get; init; }

    [FromQuery]
    public Guid? ProductId { get; init; }

    [FromQuery]
    public SaleStatus? Status { get; init; }

    [FromQuery(Name = "_minTotalAmount")]
    public decimal? MinTotalAmount { get; init; }

    [FromQuery(Name = "_maxTotalAmount")]
    public decimal? MaxTotalAmount { get; init; }

    [FromQuery(Name = "_minCreatedAt")]
    public DateTime? MinCreatedAt { get; init; }

    [FromQuery(Name = "_maxCreatedAt")]
    public DateTime? MaxCreatedAt { get; init; }

    [FromQuery(Name = "_order")]
    public string? Order { get; init; }

    public ListSalesQuery ToApplicationQuery() => new(
        PageNumber,
        PageSize,
        Number,
        CustomerId,
        BranchId,
        ProductId,
        Status,
        MinTotalAmount,
        MaxTotalAmount,
        MinCreatedAt,
        MaxCreatedAt,
        Order);
}
