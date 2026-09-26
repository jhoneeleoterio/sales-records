using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact]
    public void Create_WithValidData_ShouldSetSaleProperties()
    {
        var data = SaleTestData.GenerateValidData();

        var sale = Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            data.Items);

        Assert.Equal(data.CustomerId, sale.CustomerId);
        Assert.Equal(data.CustomerName, sale.CustomerName);
        Assert.Equal(data.BranchId, sale.BranchId);
        Assert.Equal(data.BranchName, sale.BranchName);
        Assert.Equal(SaleStatus.NotCancelled, sale.Status);
        Assert.Single(sale.Items);
        Assert.All(data.Items, item =>
            Assert.Contains(item, sale.Items));
    }

    [Fact]
    public void Create_ShouldCalculateSaleAggregatesFromItems()
    {
        var data = SaleTestData.GenerateValidData();
        var firstItem = SaleItem.Create(Guid.NewGuid(), "Notebook", 100m, 4);
        var secondItem = SaleItem.Create(Guid.NewGuid(), "Mouse", 50m, 10);

        var sale = Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            [firstItem, secondItem]);

        Assert.Equal(14, sale.Quantity);
        Assert.Equal(140m, sale.Discount);
        Assert.Equal(760m, sale.TotalAmount);
    }

    [Fact]
    public void Create_ShouldNotShareItemsBetweenSales()
    {
        var firstItem = SaleItem.Create(Guid.NewGuid(), "Notebook", 100m, 1);
        var secondItem = SaleItem.Create(Guid.NewGuid(), "Mouse", 50m, 1);

        var firstSale = Sale.Create(
            Guid.NewGuid(), "Cliente A", Guid.NewGuid(), "Filial A", [firstItem]);

        var secondSale = Sale.Create(
            Guid.NewGuid(), "Cliente B", Guid.NewGuid(), "Filial B", [secondItem]);

        Assert.Single(firstSale.Items);
        Assert.Contains(firstItem, firstSale.Items);

        Assert.Single(secondSale.Items);
        Assert.Contains(secondItem, secondSale.Items);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowDomainException()
    {
        var data = SaleTestData.GenerateValidData();
        
        var action = () => Sale.Create(
            Guid.Empty,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            data.Items);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Create_WithEmptyBranchId_ShouldThrowDomainException()
    {
        var data = SaleTestData.GenerateValidData();
        
        var action = () => Sale.Create(
            data.CustomerId,
            data.CustomerName,
            Guid.Empty,
            data.BranchName,
            data.Items);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Create_WithNullItems_ShouldThrowDomainException()
    {
        var data = SaleTestData.GenerateValidData();
        
        var action = () => Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            null!);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Create_WithEmptyItems_ShouldThrowDomainException()
    {
        var data = SaleTestData.GenerateValidData();
        
        var action = () => Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            []);
        
        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var data = SaleTestData.GenerateValidData();
        
        var sale = Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            data.Items);

        sale.Cancel();

        Assert.Equal(SaleStatus.Cancelled, sale.Status);
        Assert.NotNull(sale.UpdatedAt);
    }

    [Fact]
    public void Update_WithReplacementItems_ShouldRecalculateSaleAggregates()
    {
        var data = SaleTestData.GenerateValidData();
        var sale = Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            data.Items);

        var items = new List<SaleItem>
        {
            SaleItem.Create(Guid.NewGuid(), "Notebook", 100m, 4),
            SaleItem.Create(Guid.NewGuid(), "Mouse", 50m, 2)
        };

        sale.Update(data.CustomerId, "Cliente Atualizado", null, null, items);

        Assert.Equal("Cliente Atualizado", sale.CustomerName);
        Assert.Equal(6, sale.Quantity);
        Assert.Equal(40m, sale.Discount);
        Assert.Equal(460m, sale.TotalAmount);
        Assert.Equal(2, sale.Items.Count);
        Assert.NotNull(sale.UpdatedAt);
    }

    [Fact]
    public void Update_WithOnlyCustomerId_ShouldThrowDomainException()
    {
        var data = SaleTestData.GenerateValidData();
        var sale = Sale.Create(
            data.CustomerId,
            data.CustomerName,
            data.BranchId,
            data.BranchName,
            data.Items);

        var action = () => sale.Update(Guid.NewGuid(), null, null, null, null);

        Assert.Throws<DomainException>(action);
    }
}
