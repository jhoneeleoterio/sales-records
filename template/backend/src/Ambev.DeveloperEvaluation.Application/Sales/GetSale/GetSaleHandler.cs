using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using OneOf;
using Ambev.DeveloperEvaluation.Application.Sales;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public sealed class GetSaleHandler(ISaleRepository repository)
    : IRequestHandler<GetSaleQuery, OneOf<GetSaleResult, SaleNotFoundResult>>
{
    public async Task<OneOf<GetSaleResult, SaleNotFoundResult>> Handle(
        GetSaleQuery query,
        CancellationToken cancellationToken)
    {
        var sale = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (sale is null)
        {
            return new SaleNotFoundResult(query.Id);
        }

        return new GetSaleResult(
            sale.Id,
            sale.Number,
            sale.CustomerId,
            sale.CustomerName,
            sale.BranchId,
            sale.BranchName,
            sale.Quantity,
            sale.TotalAmount,
            sale.Discount,
            sale.Status,
            sale.CreatedAt,
            sale.UpdatedAt,
            sale.Items
                .Select(item => new GetSaleItemResult(
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity,
                    item.Discount,
                    item.TotalAmount))
                .ToList());
    }
}
