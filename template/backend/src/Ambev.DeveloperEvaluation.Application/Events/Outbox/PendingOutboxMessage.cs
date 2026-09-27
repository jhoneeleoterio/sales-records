namespace Ambev.DeveloperEvaluation.Application.Events.Outbox;

public sealed record PendingOutboxMessage(
    Guid Id,
    Guid EventId,
    DateTime OccurredAt,
    string Type,
    string Payload,
    int RetryCount);
