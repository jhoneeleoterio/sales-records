using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Theory]
    [InlineData(3, 0, 300)]
    [InlineData(4, 40, 360)]
    [InlineData(9, 90, 810)]
    [InlineData(10, 200, 800)]
    [InlineData(20, 400, 1600)]
    public void Create_ShouldCalculateDiscountAndTotalCorrectly(
        int quantity,
        decimal expectedDiscount,
        decimal expectedTotal)
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Notebook";
        var unitPrice = 100m;

        // Act
        var item = SaleItem.Create(productId, productName, unitPrice, quantity);

        // Assert
        Assert.Equal(expectedDiscount, item.Discount);
        Assert.Equal(expectedTotal, item.TotalAmount);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    public void Create_WithInvalidQuantity_ShouldThrowDomainException(
        int quantity)
    {
        // Act
        var action = () => SaleItemTestData.CreateItem(quantity);

        // Assert
        Assert.Throws<DomainException>(action);
    }
    
    [Fact]
    public void Create_WithValidData_ShouldSetItemProperties()
    {
        var productId = Guid.NewGuid();

        var item = SaleItem.Create(productId, "Notebook", 100m, 4);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal("Notebook", item.ProductName);
        Assert.Equal(100m, item.UnitPrice);
        Assert.Equal(4, item.Quantity);
        Assert.Equal(40m, item.Discount);
        Assert.Equal(360m, item.TotalAmount);
    }
    
    [Fact]
    public void Create_WithEmptyProductId_ShouldThrowDomainException()
    {
        var action = () => SaleItem.Create(Guid.Empty, "Notebook", 100m, 1);

        Assert.Throws<DomainException>(action);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidProductName_ShouldThrowDomainException(string? productName)
    {
        var action = () => SaleItem.Create(Guid.NewGuid(), productName!, 100m, 1);

        Assert.Throws<DomainException>(action);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidUnitPrice_ShouldThrowDomainException(decimal unitPrice)
    {
        var action = () => SaleItem.Create(Guid.NewGuid(), "Notebook", unitPrice, 1);

        Assert.Throws<DomainException>(action);
    }
}