using Ambev.DeveloperEvaluation.Application.Events.Integration;
using Ambev.DeveloperEvaluation.Messaging.Rebus;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class RebusIntegrationEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_ShouldPublishIntegrationEventEnvelope()
    {
        var bus = Substitute.For<IBus>();
        var publisher = new RebusIntegrationEventPublisher(bus);
        var integrationEvent = new IntegrationEventEnvelope(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "SaleCreated",
            "{\"saleId\":\"sample\"}");

        await publisher.PublishAsync(integrationEvent);

        await bus.Received(1).Publish(integrationEvent);
    }
}
