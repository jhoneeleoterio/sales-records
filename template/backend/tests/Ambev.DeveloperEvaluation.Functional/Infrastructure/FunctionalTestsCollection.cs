using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Infrastructure;

/// <summary>
/// Serializes functional tests that share one PostgreSQL database and Web API host.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class FunctionalTestsCollection :
    ICollectionFixture<FunctionalDatabaseFixture>,
    ICollectionFixture<FunctionalWebApplicationFactory>
{
    public const string Name = "Functional tests";
}
