using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public sealed class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(command => command)
            .Must(command =>
                command.CustomerId.HasValue ||
                command.CustomerName is not null ||
                command.BranchId.HasValue ||
                command.BranchName is not null ||
                command.Items is not null)
            .WithMessage("At least one field must be provided for update.");

        RuleFor(command => command)
            .Must(command => command.CustomerId.HasValue == (command.CustomerName is not null))
            .WithMessage("Customer ID and customer name must be provided together.");

        RuleFor(command => command)
            .Must(command => command.BranchId.HasValue == (command.BranchName is not null))
            .WithMessage("Branch ID and branch name must be provided together.");

        When(command => command.CustomerId.HasValue, () =>
        {
            RuleFor(command => command.CustomerId)
                .NotEqual(Guid.Empty);
        });

        When(command => command.CustomerName is not null, () =>
        {
            RuleFor(command => command.CustomerName)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(command => command.BranchId.HasValue, () =>
        {
            RuleFor(command => command.BranchId)
                .NotEqual(Guid.Empty);
        });

        When(command => command.BranchName is not null, () =>
        {
            RuleFor(command => command.BranchName)
                .NotEmpty()
                .MaximumLength(100);
        });

        When(command => command.Items is not null, () =>
        {
            RuleFor(command => command.Items)
                .NotEmpty();

            RuleForEach(command => command.Items)
                .SetValidator(new UpdateSaleItemValidator());
        });
    }
}

public sealed class UpdateSaleItemValidator : AbstractValidator<UpdateSaleItemCommand>
{
    public UpdateSaleItemValidator()
    {
        RuleFor(item => item.ProductId).NotEqual(Guid.Empty);
        RuleFor(item => item.ProductName).NotEmpty().MaximumLength(100);
        RuleFor(item => item.UnitPrice).GreaterThan(0);
        RuleFor(item => item.Quantity).InclusiveBetween(1, 20);
    }
}
