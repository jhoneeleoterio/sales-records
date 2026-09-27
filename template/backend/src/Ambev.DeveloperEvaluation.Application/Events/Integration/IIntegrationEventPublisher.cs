namespace Ambev.DeveloperEvaluation.Application.Events.Integration;

/// <summary>
/// Publishes integration events through an external transport.
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync(
        IntegrationEventEnvelope integrationEvent,
        CancellationToken cancellationToken = default);
}
