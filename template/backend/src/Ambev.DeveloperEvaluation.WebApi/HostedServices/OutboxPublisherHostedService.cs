using Ambev.DeveloperEvaluation.Application.Events.Outbox;
using MediatR;

namespace Ambev.DeveloperEvaluation.WebApi.HostedServices;

public sealed class OutboxPublisherHostedService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

                await mediator.Send(new ProcessOutboxMessagesCommand(), stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred while publishing outbox messages.");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }
}
