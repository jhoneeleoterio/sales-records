namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record SaleCancelledEvent(
    Guid SaleId,
    DateTime CancelledAt) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}
