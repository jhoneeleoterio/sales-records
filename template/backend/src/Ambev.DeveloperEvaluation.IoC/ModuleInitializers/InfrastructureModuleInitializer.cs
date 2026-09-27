using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Ambev.DeveloperEvaluation.Application.Events.Outbox;
using Ambev.DeveloperEvaluation.Application.Events.Integration;
using Ambev.DeveloperEvaluation.Messaging.Rebus;
using Ambev.DeveloperEvaluation.ORM.Outbox;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class InfrastructureModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<DefaultContext>());
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ISaleRepository, SaleRepository>();
        builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();

        if (builder.Configuration.GetValue<bool>("RabbitMq:Enabled"))
        {
            builder.Services.AddRabbitMqMessaging(builder.Configuration);
        }
        else
        {
            builder.Services.AddSingleton<IIntegrationEventPublisher, RabbitMqDisabledIntegrationEventPublisher>();
        }
    }
}
