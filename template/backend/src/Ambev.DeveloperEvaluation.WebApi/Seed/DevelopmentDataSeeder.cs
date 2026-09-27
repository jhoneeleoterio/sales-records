using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.WebApi.Seed;

/// <summary>
/// Loads sample data only for a new development database.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        DefaultContext context,
        ISaleRepository saleRepository,
        CancellationToken cancellationToken = default)
    {
        if (await context.Sales.AnyAsync(cancellationToken))
        {
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var sales = new List<Sale>
        {
            Sale.Create(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Sample Customer",
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Main Branch",
                [
                    SaleItem.Create(
                        Guid.Parse("00000000-0000-0000-0000-000000000001"),
                        "Notebook",
                        1000m,
                        4)
                ]),
            Sale.Create(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "Contoso Ltd.",
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                "North Branch",
                [
                    SaleItem.Create(
                        Guid.Parse("00000000-0000-0000-0000-000000000002"),
                        "Monitor",
                        800m,
                        2),
                    SaleItem.Create(
                        Guid.Parse("00000000-0000-0000-0000-000000000003"),
                        "Mouse",
                        100m,
                        10)
                ]),
            Sale.Create(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Fabrikam Inc.",
                Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                "South Branch",
                [
                    SaleItem.Create(
                        Guid.Parse("00000000-0000-0000-0000-000000000004"),
                        "Keyboard",
                        300m,
                        1)
                ])
        };

        sales[2].Cancel();

        foreach (var sale in sales)
        {
            await saleRepository.CreateAsync(sale, cancellationToken);
            sale.ClearDomainEvents();
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
