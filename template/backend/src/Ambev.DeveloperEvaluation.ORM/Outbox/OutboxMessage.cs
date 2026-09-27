using Ambev.DeveloperEvaluation.Domain.Events;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

/// <summary>
/// Represents an integration message durably recorded with a business transaction.
/// </summary>
public class OutboxMessage
{
    private OutboxMessage()
    {
    }

    private OutboxMessage(
        Guid id,
        Guid eventId,
        DateTime occurredAt,
        string type,
        string payload)
    {
        Id = id;
        EventId = eventId;
        OccurredAt = occurredAt;
        Type = type;
        Payload = payload;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextAttemptAt { get; private set; }
    public string? Error { get; private set; }

    public static OutboxMessage Create(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();

        return new OutboxMessage(
            Guid.NewGuid(),
            domainEvent.EventId,
            DateTime.UtcNow,
            eventType.FullName ?? eventType.Name,
            JsonSerializer.Serialize(domainEvent, eventType));
    }

    public void MarkAsProcessed(DateTime processedAt)
    {
        ProcessedAt = processedAt;
        Error = null;
    }

    public void MarkAsFailed(string error, DateTime nextAttemptAt)
    {
        RetryCount++;
        Error = error;
        NextAttemptAt = nextAttemptAt;
    }
}
