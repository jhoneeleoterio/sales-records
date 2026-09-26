using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository(DefaultContext context) : ISaleRepository
{
    /// <summary>
    /// Creates a new sale in the database
    /// </summary>
    /// <param name="sale"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await context.Sales.AddAsync(sale, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return sale;
    }

    /// <inheritdoc />
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Sales
            .AsNoTracking()
            .Include(sale => sale.Items)
            .FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Sale?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Sales
            .Include(sale => sale.Items)
            .FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyCollection<Sale> Sales, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        SaleFilter filter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Sale> sales = context.Sales.AsNoTracking();

        if (filter.Number.HasValue)
        {
            sales = sales.Where(sale => sale.Number == filter.Number.Value);
        }

        if (filter.CustomerId.HasValue)
        {
            sales = sales.Where(sale => sale.CustomerId == filter.CustomerId.Value);
        }

        if (filter.BranchId.HasValue)
        {
            sales = sales.Where(sale => sale.BranchId == filter.BranchId.Value);
        }

        if (filter.ProductId.HasValue)
        {
            sales = sales.Where(sale => sale.Items.Any(item => item.ProductId == filter.ProductId.Value));
        }

        if (filter.Status.HasValue)
        {
            sales = sales.Where(sale => sale.Status == filter.Status.Value);
        }

        if (filter.MinTotalAmount.HasValue)
        {
            sales = sales.Where(sale => sale.TotalAmount >= filter.MinTotalAmount.Value);
        }

        if (filter.MaxTotalAmount.HasValue)
        {
            sales = sales.Where(sale => sale.TotalAmount <= filter.MaxTotalAmount.Value);
        }

        if (filter.MinCreatedAt.HasValue)
        {
            sales = sales.Where(sale => sale.CreatedAt >= filter.MinCreatedAt.Value);
        }

        if (filter.MaxCreatedAt.HasValue)
        {
            sales = sales.Where(sale => sale.CreatedAt <= filter.MaxCreatedAt.Value);
        }

        var orderedSales = ApplyOrdering(sales, filter.Order);

        var totalCount = await orderedSales.CountAsync(cancellationToken);
        var items = await orderedSales
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IOrderedQueryable<Sale> ApplyOrdering(
        IQueryable<Sale> sales,
        string? order)
    {
        var segments = string.IsNullOrWhiteSpace(order)
            ? ["number"]
            : order.Split(',', StringSplitOptions.TrimEntries);

        IOrderedQueryable<Sale>? orderedSales = null;

        foreach (var segment in segments)
        {
            var tokens = segment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = tokens[0].ToLowerInvariant();
            var descending = tokens.Length == 2 &&
                tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            orderedSales = orderedSales is null
                ? OrderBy(sales, field, descending)
                : ThenBy(orderedSales, field, descending);
        }

        return orderedSales!.ThenBy(sale => sale.Id);
    }

    private static IOrderedQueryable<Sale> OrderBy(
        IQueryable<Sale> sales,
        string field,
        bool descending) =>
        (field, descending) switch
        {
            ("number", true) => sales.OrderByDescending(sale => sale.Number),
            ("createdat", true) => sales.OrderByDescending(sale => sale.CreatedAt),
            ("totalamount", true) => sales.OrderByDescending(sale => sale.TotalAmount),
            ("number", false) => sales.OrderBy(sale => sale.Number),
            ("createdat", false) => sales.OrderBy(sale => sale.CreatedAt),
            ("totalamount", false) => sales.OrderBy(sale => sale.TotalAmount),
            _ => throw new InvalidOperationException("Unsupported sale ordering field.")
        };

    private static IOrderedQueryable<Sale> ThenBy(
        IOrderedQueryable<Sale> sales,
        string field,
        bool descending) =>
        (field, descending) switch
        {
            ("number", true) => sales.ThenByDescending(sale => sale.Number),
            ("createdat", true) => sales.ThenByDescending(sale => sale.CreatedAt),
            ("totalamount", true) => sales.ThenByDescending(sale => sale.TotalAmount),
            ("number", false) => sales.ThenBy(sale => sale.Number),
            ("createdat", false) => sales.ThenBy(sale => sale.CreatedAt),
            ("totalamount", false) => sales.ThenBy(sale => sale.TotalAmount),
            _ => throw new InvalidOperationException("Unsupported sale ordering field.")
        };

    /// <inheritdoc />
    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        if (context.Entry(sale).State == EntityState.Detached)
        {
            throw new InvalidOperationException("The sale must be tracked before it can be updated.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        if (context.Entry(sale).State == EntityState.Detached)
        {
            throw new InvalidOperationException("The sale must be tracked before it can be deleted.");
        }

        context.Sales.Remove(sale);
        await context.SaveChangesAsync(cancellationToken);
    }
}
