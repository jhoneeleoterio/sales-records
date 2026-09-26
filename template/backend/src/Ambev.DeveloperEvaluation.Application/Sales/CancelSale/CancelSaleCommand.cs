using MediatR;
using OneOf;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public sealed record CancelSaleCommand(Guid Id)
    : IRequest<OneOf<CancelSaleResult, SaleNotFoundResult>>;
