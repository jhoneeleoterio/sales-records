using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler(ISaleRepository repository) : IRequestHandler<CreateSaleCommand, Guid>
{
    /// <summary>
    /// Handles the CreateSaleCommand command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<Guid> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = Sale.Create(
            command.CustomerId, 
            command.CustomerName, 
            command.BranchId, 
            command.BranchName,
            [
                .. command.Items.Select(s => SaleItem.Create(s.ProductId, s.ProductName, s.UnitPrice, s.Quantity))
            ]);
        
        await repository.CreateAsync(sale, cancellationToken);
        return sale.Id;
    }
}