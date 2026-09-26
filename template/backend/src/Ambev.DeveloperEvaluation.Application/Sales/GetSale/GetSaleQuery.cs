using MediatR;
using OneOf;
using Ambev.DeveloperEvaluation.Application.Sales;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public sealed record GetSaleQuery(Guid Id) : IRequest<OneOf<GetSaleResult, SaleNotFoundResult>>;
