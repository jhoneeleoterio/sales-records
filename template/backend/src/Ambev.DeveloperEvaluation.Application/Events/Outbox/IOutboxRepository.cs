namespace Ambev.DeveloperEvaluation.Application.Events.Outbox;

/// <summary>
/// Provides access to integration messages awaiting publication.
/// </summary>
public interface IOutboxRepository
{
    Task<IReadOnlyCollection<PendingOutboxMessage>> GetPendingAsync(
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(
        Guid id,
        DateTime processedAt,
        CancellationToken cancellationToken = default);

    Task MarkAsFailedAsync(
        Guid id,
        string error,
        DateTime nextAttemptAt,
        CancellationToken cancellationToken = default);
}
