namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record SaleCancelledEvent(
    Guid SaleId,
    DateTime CancelledAt) : IDomainEvent;
