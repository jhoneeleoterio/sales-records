using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class CreateSaleHandlerTestData
{
    private static readonly Faker<CreateSaleItemCommand> ItemFaker =
        new Faker<CreateSaleItemCommand>()
            .RuleFor(item => item.ProductId, faker => faker.Random.Guid())
            .RuleFor(item => item.ProductName, faker => faker.Commerce.ProductName())
            .RuleFor(item => item.UnitPrice, faker => faker.Random.Decimal(1, 1_000))
            .RuleFor(item => item.Quantity, faker => faker.Random.Int(1, 20));

    private static readonly Faker<CreateSaleCommand> CommandFaker =
        new Faker<CreateSaleCommand>()
            .RuleFor(sale => sale.CustomerId, faker => faker.Random.Guid())
            .RuleFor(sale => sale.CustomerName, faker => faker.Person.FullName)
            .RuleFor(sale => sale.BranchId, faker => faker.Random.Guid())
            .RuleFor(sale => sale.BranchName, faker => faker.Company.CompanyName())
            .RuleFor(sale => sale.Items, _ => ItemFaker.Generate(2));

    public static CreateSaleCommand GenerateValidCommand()
        => CommandFaker.Generate();
}