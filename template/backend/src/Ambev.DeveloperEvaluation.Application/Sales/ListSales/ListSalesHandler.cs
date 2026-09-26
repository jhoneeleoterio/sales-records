using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed class ListSalesHandler(ISaleRepository repository)
    : IRequestHandler<ListSalesQuery, ListSalesResult>
{
    public async Task<ListSalesResult> Handle(
        ListSalesQuery query,
        CancellationToken cancellationToken)
    {
        var (sales, totalCount) = await repository.GetAllAsync(
            query.PageNumber,
            query.PageSize,
            new SaleFilter(
                query.Number,
                query.CustomerId,
                query.BranchId,
                query.ProductId,
                query.Status,
                query.MinTotalAmount,
                query.MaxTotalAmount,
                query.MinCreatedAt,
                query.MaxCreatedAt,
                query.Order),
            cancellationToken);

        var items = sales
            .Select(sale => new ListSalesItemResult(
                sale.Id,
                sale.Number,
                sale.CustomerId,
                sale.CustomerName,
                sale.BranchId,
                sale.BranchName,
                sale.TotalAmount,
                sale.Status,
                sale.CreatedAt))
            .ToList();

        return new ListSalesResult(items, totalCount);
    }
}
