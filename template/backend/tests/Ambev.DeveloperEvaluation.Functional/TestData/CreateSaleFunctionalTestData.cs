using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Functional.TestData;

public static class CreateSaleFunctionalTestData
{
    private static readonly Faker Faker = new("pt_BR");

    public static CreateSaleCommand GenerateValidCommand(int itemCount = 2)
    {
        var items = Enumerable.Range(0, itemCount)
            .Select(_ => new CreateSaleItemCommand
            {
                ProductId = Faker.Random.Guid(),
                ProductName = Faker.Commerce.ProductName(),
                UnitPrice = Math.Round(
                    Faker.Random.Decimal(1m, 1_000m),
                    2,
                    MidpointRounding.AwayFromZero),
                Quantity = Faker.Random.Int(1, 20)
            })
            .ToList();

        return new CreateSaleCommand
        {
            CustomerId = Faker.Random.Guid(),
            CustomerName = Faker.Name.FullName(),
            BranchId = Faker.Random.Guid(),
            BranchName = Faker.Company.CompanyName(),
            Items = items
        };
    }
}
