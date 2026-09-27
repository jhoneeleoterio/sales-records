namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed record SaleUpdatedEvent(
    Guid SaleId,
    DateTime UpdatedAt) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}
