using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ambev.DeveloperEvaluation.Integration.Infrastructure;

public sealed class IntegrationDatabase
{
    private readonly DbContextOptions<DefaultContext> _options;

    public IntegrationDatabase()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Integration.json", optional: false)
            .Build();

        var connectionString =
            configuration.GetConnectionString("IntegrationTestConnection")
            ?? throw new InvalidOperationException(
                "A connection string IntegrationTestConnection não foi configurada.");

        _options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    public async Task<DefaultContext> CreateAsync()
    {
        var context = new DefaultContext(_options);

        try
        {
            if (!await context.Database.CanConnectAsync())
                throw new InvalidOperationException(
                    "Não foi possível conectar ao banco de integração.");

            await context.Database.MigrateAsync();

            return context;
        }
        catch
        {
            await context.DisposeAsync();
            throw;
        }
    }
}