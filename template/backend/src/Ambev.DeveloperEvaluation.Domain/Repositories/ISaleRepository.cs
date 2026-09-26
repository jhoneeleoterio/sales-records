using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository
{
    /// <summary>
    /// Create a new sale in the repository
    /// </summary>
    /// <param name="sale"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale and its items by identifier.
    /// </summary>
    /// <param name="id">The sale identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sale if found; otherwise, null.</returns>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tracked sale and its items for a command that will modify it.
    /// </summary>
    Task<Sale?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a filtered and ordered page of sales.
    /// </summary>
    Task<(IReadOnlyCollection<Sale> Sales, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        SaleFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made to a tracked sale.
    /// </summary>
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a tracked sale from the repository.
    /// </summary>
    Task DeleteAsync(Sale sale, CancellationToken cancellationToken = default);
}
