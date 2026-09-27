namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record SaleCreatedEvent(
    Guid SaleId,
    DateTime CreatedAt) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}
