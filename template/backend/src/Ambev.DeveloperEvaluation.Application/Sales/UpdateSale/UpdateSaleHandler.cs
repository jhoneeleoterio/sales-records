using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using OneOf;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public sealed class UpdateSaleHandler(
    ISaleRepository repository,
    IDomainEventDispatcher domainEventDispatcher)
    : IRequestHandler<UpdateSaleCommand, OneOf<UpdateSaleResult, SaleNotFoundResult>>
{
    public async Task<OneOf<UpdateSaleResult, SaleNotFoundResult>> Handle(
        UpdateSaleCommand command,
        CancellationToken cancellationToken)
    {
        var sale = await repository.GetByIdForUpdateAsync(command.Id, cancellationToken);

        if (sale is null)
        {
            return new SaleNotFoundResult(command.Id);
        }

        var items = command.Items?
            .Select(item => SaleItem.Create(
                item.ProductId,
                item.ProductName,
                item.UnitPrice,
                item.Quantity))
            .ToList();

        sale.Update(
            command.CustomerId,
            command.CustomerName,
            command.BranchId,
            command.BranchName,
            items);

        await repository.UpdateAsync(sale, cancellationToken);
        var domainEvents = sale.DomainEvents.ToArray();
        await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return new UpdateSaleResult(sale.Id, sale.UpdatedAt);
    }
}
