using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public sealed record SaleData(
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    List<SaleItem> Items);

public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static SaleData GenerateValidData(List<SaleItem>? items = null)
    {
        return new SaleData(
            CustomerId: Faker.Random.Guid(),
            CustomerName: Faker.Name.FullName(),
            BranchId: Faker.Random.Guid(),
            BranchName: Faker.Company.CompanyName(),
            Items: items ?? GenerateValidItems());
    }

    private static List<SaleItem> GenerateValidItems(int quantity = 1)
    {
        return
        [
            SaleItemTestData.CreateItem(
                quantity: quantity,
                unitPrice: Faker.Random.Decimal(1m, 1_000m))
        ];
    }

    private static Sale CreateValidSale(List<SaleItem>? items = null)
    {
        var data = GenerateValidData(items);

        return Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            data.Items);
    }
}