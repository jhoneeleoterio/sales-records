using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;
using OneOf;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public sealed class DeleteSaleHandler(ISaleRepository repository)
    : IRequestHandler<DeleteSaleCommand, OneOf<DeleteSaleResult, SaleNotFoundResult>>
{
    public async Task<OneOf<DeleteSaleResult, SaleNotFoundResult>> Handle(
        DeleteSaleCommand command,
        CancellationToken cancellationToken)
    {
        var sale = await repository.GetByIdForUpdateAsync(command.Id, cancellationToken);

        if (sale is null)
        {
            return new SaleNotFoundResult(command.Id);
        }

        await repository.DeleteAsync(sale, cancellationToken);

        return new DeleteSaleResult(command.Id);
    }
}
