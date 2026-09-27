using Ambev.DeveloperEvaluation.Application.Events.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class OutboxRepository(DefaultContext context) : IOutboxRepository
{
    public async Task<IReadOnlyCollection<PendingOutboxMessage>> GetPendingAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessages
            .AsNoTracking()
            .Where(message =>
                message.ProcessedAt == null &&
                (message.NextAttemptAt == null || message.NextAttemptAt <= utcNow))
            .OrderBy(message => message.OccurredAt)
            .Take(batchSize)
            .Select(message => new PendingOutboxMessage(
                message.Id,
                message.EventId,
                message.OccurredAt,
                message.Type,
                message.Payload,
                message.RetryCount))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(
        Guid id,
        DateTime processedAt,
        CancellationToken cancellationToken = default)
    {
        var message = await context.OutboxMessages.FindAsync([id], cancellationToken)
            ?? throw new InvalidOperationException($"Outbox message {id} was not found.");

        message.MarkAsProcessed(processedAt);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsFailedAsync(
        Guid id,
        string error,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken = default)
    {
        var message = await context.OutboxMessages.FindAsync([id], cancellationToken)
            ?? throw new InvalidOperationException($"Outbox message {id} was not found.");

        message.MarkAsFailed(error, nextAttemptAt);
        await context.SaveChangesAsync(cancellationToken);
    }
}
