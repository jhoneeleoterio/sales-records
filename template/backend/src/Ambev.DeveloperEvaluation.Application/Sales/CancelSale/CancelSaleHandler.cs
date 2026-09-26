using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using OneOf;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public sealed class CancelSaleHandler(
    ISaleRepository repository,
    IDomainEventDispatcher domainEventDispatcher)
    : IRequestHandler<CancelSaleCommand, OneOf<CancelSaleResult, SaleNotFoundResult>>
{
    public async Task<OneOf<CancelSaleResult, SaleNotFoundResult>> Handle(
        CancelSaleCommand command,
        CancellationToken cancellationToken)
    {
        var sale = await repository.GetByIdForUpdateAsync(command.Id, cancellationToken);

        if (sale is null)
        {
            return new SaleNotFoundResult(command.Id);
        }

        sale.Cancel();
        await repository.UpdateAsync(sale, cancellationToken);
        var domainEvents = sale.DomainEvents.ToArray();
        await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return new CancelSaleResult(sale.Id, sale.Status, sale.UpdatedAt);
    }
}
