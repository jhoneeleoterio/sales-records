using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Integration.TestData;

public static class SaleIntegrationTestData
{
    private static readonly Faker Faker = new("pt_BR");

    public static Sale GenerateValidSale(int itemCount = 2)
    {
        var items = Enumerable.Range(0, itemCount)
            .Select(_ => SaleItem.Create(
                Faker.Random.Guid(),
                Faker.Commerce.ProductName(),
                Math.Round(
                    Faker.Random.Decimal(1m, 1_000m),
                    2,
                    MidpointRounding.AwayFromZero),
                Faker.Random.Int(1, 20)))
            .ToList();

        return Sale.Create(
            Faker.Random.Guid(),
            Faker.Name.FullName(),
            Faker.Random.Guid(),
            Faker.Company.CompanyName(),
            items);
    }
}