using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

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
}