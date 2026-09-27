using Ambev.DeveloperEvaluation.Application.Events.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.ServiceProvider;

namespace Ambev.DeveloperEvaluation.Messaging.Rebus;

public static class RebusMessagingExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["RabbitMq:ConnectionString"]
            ?? throw new InvalidOperationException(
                "RabbitMq:ConnectionString must be configured when RabbitMQ is enabled.");

        var inputQueue = configuration["RabbitMq:InputQueue"]
            ?? "sales-records";

        services.AddRebus(configure => configure
            .Transport(transport => transport.UseRabbitMq(connectionString, inputQueue)));

        services.AddSingleton<IIntegrationEventPublisher, RebusIntegrationEventPublisher>();

        return services;
    }
}
