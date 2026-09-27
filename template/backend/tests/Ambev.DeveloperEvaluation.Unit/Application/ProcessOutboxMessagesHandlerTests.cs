using Ambev.DeveloperEvaluation.Application.Events.Integration;
using Ambev.DeveloperEvaluation.Application.Events.Outbox;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class ProcessOutboxMessagesHandlerTests
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;
    private readonly ProcessOutboxMessagesHandler _handler;

    public ProcessOutboxMessagesHandlerTests()
    {
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _integrationEventPublisher = Substitute.For<IIntegrationEventPublisher>();
        _handler = new ProcessOutboxMessagesHandler(
            _outboxRepository,
            _integrationEventPublisher);
    }

    [Fact]
    public async Task Handle_WhenPublicationSucceeds_ShouldMarkMessageAsProcessed()
    {
        var message = CreatePendingMessage();
        _outboxRepository
            .GetPendingAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([message]);

        var result = await _handler.Handle(
            new ProcessOutboxMessagesCommand(),
            CancellationToken.None);

        Assert.Equal(1, result);
        await _integrationEventPublisher.Received(1).PublishAsync(
            Arg.Is<IntegrationEventEnvelope>(integrationEvent =>
                integrationEvent.EventId == message.EventId &&
                integrationEvent.Payload == message.Payload),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).MarkAsProcessedAsync(
            message.Id,
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPublicationFails_ShouldScheduleMessageForRetry()
    {
        var message = CreatePendingMessage(retryCount: 2);
        _outboxRepository
            .GetPendingAsync(Arg.Any<int>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([message]);
        _integrationEventPublisher
            .PublishAsync(Arg.Any<IntegrationEventEnvelope>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("RabbitMQ is unavailable.")));

        await _handler.Handle(
            new ProcessOutboxMessagesCommand(),
            CancellationToken.None);

        await _outboxRepository.Received(1).MarkAsFailedAsync(
            message.Id,
            "RabbitMQ is unavailable.",
            Arg.Is<DateTime>(nextAttemptAt => nextAttemptAt > DateTime.UtcNow),
            Arg.Any<CancellationToken>());
        await _outboxRepository.DidNotReceive().MarkAsProcessedAsync(
            Arg.Any<Guid>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    private static PendingOutboxMessage CreatePendingMessage(int retryCount = 0) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "SaleCreated",
            "{\"saleId\":\"sample\"}",
            retryCount);
}
