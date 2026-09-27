using Ambev.DeveloperEvaluation.Application.Events.Integration;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Events.Outbox;

public sealed class ProcessOutboxMessagesHandler(
    IOutboxRepository outboxRepository,
    IIntegrationEventPublisher integrationEventPublisher)
    : IRequestHandler<ProcessOutboxMessagesCommand, int>
{
    public async Task<int> Handle(
        ProcessOutboxMessagesCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var messages = await outboxRepository.GetPendingAsync(
            command.BatchSize,
            now,
            cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await integrationEventPublisher.PublishAsync(
                    new IntegrationEventEnvelope(
                        message.EventId,
                        message.OccurredAt,
                        message.Type,
                        message.Payload),
                    cancellationToken);

                await outboxRepository.MarkAsProcessedAsync(
                    message.Id,
                    DateTime.UtcNow,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                var retryDelay = TimeSpan.FromMinutes(
                    Math.Min(Math.Pow(2, message.RetryCount), 60));

                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    exception.Message,
                    DateTime.UtcNow.Add(retryDelay),
                    cancellationToken);
            }
        }

        return messages.Count;
    }
}
