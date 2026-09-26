using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleValidator: AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(sale => sale.CustomerId).NotEqual(Guid.Empty);
        RuleFor(sale => sale.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(sale => sale.BranchId).NotEqual(Guid.Empty);
        RuleFor(sale => sale.BranchName).NotEmpty().MaximumLength(100);
        RuleFor(sale => sale.Items).NotNull().NotEmpty();
        RuleForEach(sale => sale.Items).SetValidator(new CreateSaleItemValidator());
    }
}

public sealed class CreateSaleItemValidator : AbstractValidator<CreateSaleItemCommand>
{
    public CreateSaleItemValidator()
    {
        RuleFor(item => item.ProductId).NotEqual(Guid.Empty);
        RuleFor(item => item.ProductName).NotEmpty().MaximumLength(100);
        RuleFor(item => item.UnitPrice).GreaterThan(0);
        RuleFor(item => item.Quantity).InclusiveBetween(1, 20);
    }
}
