using Ambev.DeveloperEvaluation.Application.Events.Integration;
using Rebus.Bus;

namespace Ambev.DeveloperEvaluation.Messaging.Rebus;

public sealed class RebusIntegrationEventPublisher(IBus bus) : IIntegrationEventPublisher
{
    public Task PublishAsync(
        IntegrationEventEnvelope integrationEvent,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return bus.Publish(integrationEvent);
    }
}
