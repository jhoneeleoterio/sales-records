using MediatR;
using OneOf;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public sealed record UpdateSaleCommand(
    Guid? CustomerId = null,
    string? CustomerName = null,
    Guid? BranchId = null,
    string? BranchName = null,
    List<UpdateSaleItemCommand>? Items = null)
    : IRequest<OneOf<UpdateSaleResult, SaleNotFoundResult>>
{
    public Guid Id { get; init; }
}

public sealed record UpdateSaleItemCommand(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
