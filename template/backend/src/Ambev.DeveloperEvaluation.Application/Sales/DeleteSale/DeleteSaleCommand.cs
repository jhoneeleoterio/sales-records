using MediatR;
using OneOf;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public sealed record DeleteSaleCommand(Guid Id)
    : IRequest<OneOf<DeleteSaleResult, SaleNotFoundResult>>;
