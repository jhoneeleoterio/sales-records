using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events;

/// <summary>
/// Records the sales domain events required by the technical challenge.
/// </summary>
public sealed class SaleEventLoggingHandler(ILogger<SaleEventLoggingHandler> logger)
    : INotificationHandler<DomainEventNotification>
{
    public Task Handle(
        DomainEventNotification notification,
        CancellationToken cancellationToken)
    {
        switch (notification.DomainEvent)
        {
            case SaleCreatedEvent saleCreated:
                logger.LogInformation(
                    "Sale {SaleId} created at {CreatedAt}.",
                    saleCreated.SaleId,
                    saleCreated.CreatedAt);
                break;

            case SaleUpdatedEvent saleUpdated:
                logger.LogInformation(
                    "Sale {SaleId} updated at {UpdatedAt}.",
                    saleUpdated.SaleId,
                    saleUpdated.UpdatedAt);
                break;

            case SaleCancelledEvent saleCancelled:
                logger.LogInformation(
                    "Sale {SaleId} cancelled at {CancelledAt}.",
                    saleCancelled.SaleId,
                    saleCancelled.CancelledAt);
                break;
        }

        return Task.CompletedTask;
    }
}
