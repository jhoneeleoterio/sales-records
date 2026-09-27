namespace Ambev.DeveloperEvaluation.Application.Events.Integration;

/// <summary>
/// Stable envelope used to publish an outbox message outside this application.
/// </summary>
public sealed record IntegrationEventEnvelope(
    Guid EventId,
    DateTime OccurredAt,
    string Type,
    string Payload);
