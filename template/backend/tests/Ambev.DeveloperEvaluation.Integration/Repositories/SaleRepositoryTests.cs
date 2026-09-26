using Ambev.DeveloperEvaluation.Integration.Infrastructure;
using Ambev.DeveloperEvaluation.Integration.TestData;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

/// <summary>
/// Verifies that sales are persisted and retrieved correctly by the PostgreSQL repository.
/// </summary>
public class SaleRepositoryTests
{
    /// <summary>
    /// Persists a sale with owned items and confirms that all aggregate values are stored in the database.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ShouldPersistSaleWithItems()
    {
        var database = new IntegrationDatabase();

        await using var context = await database.CreateAsync();
        var repository = new SaleRepository(context);

        var sale = SaleIntegrationTestData.GenerateValidSale();
        await repository.CreateAsync(sale);

        context.ChangeTracker.Clear();

        var persistedSale = await context.Sales
            .Include(persisted => persisted.Items)
            .SingleAsync(persisted => persisted.Id == sale.Id);

        Assert.Equal(sale.CustomerId, persistedSale.CustomerId);
        Assert.Equal(sale.BranchId, persistedSale.BranchId);
        Assert.Equal(sale.Quantity, persistedSale.Quantity);
        Assert.Equal(sale.Discount, persistedSale.Discount);
        Assert.Equal(sale.TotalAmount, persistedSale.TotalAmount);
        Assert.Equal(sale.Items.Count, persistedSale.Items.Count);
    }

    /// <summary>
    /// Retrieves a sale through the read query and verifies that its owned items are included.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithExistingSale_ShouldReturnSaleWithItems()
    {
        var database = new IntegrationDatabase();

        await using var context = await database.CreateAsync();
        var repository = new SaleRepository(context);
        var sale = SaleIntegrationTestData.GenerateValidSale();

        await repository.CreateAsync(sale);
        context.ChangeTracker.Clear();

        var persistedSale = await repository.GetByIdAsync(sale.Id);

        Assert.NotNull(persistedSale);
        Assert.Equal(sale.Id, persistedSale.Id);
        Assert.Equal(sale.TotalAmount, persistedSale.TotalAmount);
        Assert.Equal(sale.Items.Count, persistedSale.Items.Count);
    }

    /// <summary>
    /// Filters sales by product and confirms that only sales containing that product are returned.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithProductFilter_ShouldReturnMatchingSales()
    {
        var database = new IntegrationDatabase();

        await using var context = await database.CreateAsync();
        var repository = new SaleRepository(context);
        var firstSale = SaleIntegrationTestData.GenerateValidSale();

        await repository.CreateAsync(firstSale);
        context.ChangeTracker.Clear();

        var productId = firstSale.Items.First().ProductId;
        var (sales, totalCount) = await repository.GetAllAsync(
            1,
            10,
            new SaleFilter(ProductId: productId));

        Assert.True(totalCount >= 1);
        Assert.Contains(sales, sale => sale.Id == firstSale.Id);
        Assert.All(sales, sale => Assert.Contains(sale.Items, item => item.ProductId == productId));
    }

    /// <summary>
    /// Orders sales by multiple criteria and verifies that the repository preserves the requested precedence.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithMultipleOrderCriteria_ShouldApplyTheRequestedOrder()
    {
        var database = new IntegrationDatabase();

        await using var context = await database.CreateAsync();
        var repository = new SaleRepository(context);
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var lowerTotalSale = Sale.Create(
            customerId,
            "Ordering Customer",
            branchId,
            "Ordering Branch",
            [SaleItem.Create(Guid.NewGuid(), "Product A", 10m, 1)]);
        var higherTotalSale = Sale.Create(
            customerId,
            "Ordering Customer",
            branchId,
            "Ordering Branch",
            [SaleItem.Create(Guid.NewGuid(), "Product B", 20m, 1)]);

        await repository.CreateAsync(lowerTotalSale);
        await repository.CreateAsync(higherTotalSale);
        context.ChangeTracker.Clear();

        var (sales, _) = await repository.GetAllAsync(
            1,
            10,
            new SaleFilter(
                CustomerId: customerId,
                Order: "totalAmount desc,number asc"));

        Assert.Collection(
            sales,
            sale => Assert.Equal(higherTotalSale.Id, sale.Id),
            sale => Assert.Equal(lowerTotalSale.Id, sale.Id));
    }

    /// <summary>
    /// Loads a tracked sale, cancels it, and confirms that its status and audit date are persisted.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_AfterCancellingSale_ShouldPersistStatusAndUpdatedAt()
    {
        var database = new IntegrationDatabase();

        await using var context = await database.CreateAsync();
        var repository = new SaleRepository(context);
        var sale = SaleIntegrationTestData.GenerateValidSale();

        await repository.CreateAsync(sale);
        context.ChangeTracker.Clear();

        var persistedSale = await repository.GetByIdForUpdateAsync(sale.Id);
        Assert.NotNull(persistedSale);

        persistedSale.Cancel();
        await repository.UpdateAsync(persistedSale);
        context.ChangeTracker.Clear();

        var cancelledSale = await repository.GetByIdAsync(sale.Id);

        Assert.NotNull(cancelledSale);
        Assert.Equal(SaleStatus.Cancelled, cancelledSale.Status);
        Assert.NotNull(cancelledSale.UpdatedAt);
    }

    /// <summary>
    /// Loads a tracked sale, deletes it, and verifies that the sale and its owned items are removed.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ShouldRemoveSaleWithItems()
    {
        var database = new IntegrationDatabase();

        await using var context = await database.CreateAsync();
        var repository = new SaleRepository(context);
        var sale = SaleIntegrationTestData.GenerateValidSale();

        await repository.CreateAsync(sale);
        context.ChangeTracker.Clear();

        var persistedSale = await repository.GetByIdForUpdateAsync(sale.Id);
        Assert.NotNull(persistedSale);

        await repository.DeleteAsync(persistedSale);
        context.ChangeTracker.Clear();

        var deletedSale = await repository.GetByIdAsync(sale.Id);
        var itemCount = await context.Database
            .SqlQuery<int>($"SELECT COUNT(*)::int AS \"Value\" FROM \"SaleItems\" WHERE \"SaleId\" = {sale.Id}")
            .SingleAsync();

        Assert.Null(deletedSale);
        Assert.Equal(0, itemCount);
    }
}
