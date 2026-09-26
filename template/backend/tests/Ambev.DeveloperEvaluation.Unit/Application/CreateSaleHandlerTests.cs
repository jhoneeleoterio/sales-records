using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();
        _handler = new CreateSaleHandler(_repository, _domainEventDispatcher);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAndPersistSale()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        // When
        _repository
            .CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult(callInfo.Arg<Sale>()));

        // Then
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBe(Guid.Empty);

        await _repository.Received(1).CreateAsync(
            Arg.Is<Sale>(sale =>
                sale.CustomerId == command.CustomerId &&
                sale.CustomerName == command.CustomerName &&
                sale.BranchId == command.BranchId &&
                sale.BranchName == command.BranchName &&
                sale.Items.Count == command.Items.Count &&
                sale.Quantity == command.Items.Sum(item => item.Quantity)),
            Arg.Any<CancellationToken>());

        await _domainEventDispatcher.Received(1).DispatchAsync(
            Arg.Is<IEnumerable<IDomainEvent>>(events =>
                events.OfType<SaleCreatedEvent>().Single().SaleId == result),
            Arg.Any<CancellationToken>());
    }
}
