using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _repository;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _repository = Substitute.For<ISaleRepository>();
        _handler = new CreateSaleHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAndPersistSale()
    {
        // Given
        var expectedId = Guid.NewGuid();
        var command = CreateSaleHandlerTestData.GenerateValidCommand();

        // When
        _repository
            .CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var sale = callInfo.Arg<Sale>();
                sale.Id = expectedId;

                return Task.FromResult(sale);
            });

        // Then
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(expectedId);

        await _repository.Received(1).CreateAsync(
            Arg.Is<Sale>(sale =>
                sale.CustomerId == command.CustomerId &&
                sale.CustomerName == command.CustomerName &&
                sale.BranchId == command.BranchId &&
                sale.BranchName == command.BranchName &&
                sale.Items.Count == command.Items.Count &&
                sale.Quantity == command.Items.Sum(item => item.Quantity)),
            Arg.Any<CancellationToken>());
    }
}