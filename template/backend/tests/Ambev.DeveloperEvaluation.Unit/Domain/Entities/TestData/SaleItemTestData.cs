using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleItemTestData
{
    private static readonly Faker Faker = new();

    public static SaleItem CreateItem(
        int quantity,
        decimal unitPrice = 100m)
    {
        return SaleItem.Create(
            productId: Guid.NewGuid(),
            productName: Faker.Commerce.ProductName(),
            unitPrice: unitPrice,
            quantity: quantity
        );
    }
}