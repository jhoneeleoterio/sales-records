using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Infrastructure;

/// <summary>
/// Applies migrations once before the functional test collection starts.
/// </summary>
public sealed class FunctionalDatabaseFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Functional.json", optional: false)
            .Build();

        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .Options;

        await using var context = new DefaultContext(options);
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
