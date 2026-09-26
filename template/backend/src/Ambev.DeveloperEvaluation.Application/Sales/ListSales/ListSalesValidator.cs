using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public sealed class ListSalesValidator : AbstractValidator<ListSalesQuery>
{
    public ListSalesValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Number).GreaterThan(0).When(query => query.Number.HasValue);
        RuleFor(query => query.Order)
            .Must(SaleOrdering.IsValid)
            .WithMessage("Order must use number, createdAt or totalAmount, optionally followed by asc or desc.");
        RuleFor(query => query)
            .Must(query => !query.MinTotalAmount.HasValue || !query.MaxTotalAmount.HasValue ||
                           query.MinTotalAmount <= query.MaxTotalAmount)
            .WithMessage("Minimum total amount cannot be greater than maximum total amount.");
        RuleFor(query => query)
            .Must(query => !query.MinCreatedAt.HasValue || !query.MaxCreatedAt.HasValue ||
                           query.MinCreatedAt <= query.MaxCreatedAt)
            .WithMessage("Minimum creation date cannot be greater than maximum creation date.");
    }
}
