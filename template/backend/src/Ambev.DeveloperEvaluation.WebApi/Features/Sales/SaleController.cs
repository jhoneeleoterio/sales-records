using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

public class SaleController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Retrieves a page of sales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ListSalesItemResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSales(
        [FromQuery] SaleFilterQuery filter,
        CancellationToken cancellationToken = default)
    {
        var query = filter.ToApplicationQuery();

        var validator = new ListSalesValidator();
        var validationResult = await validator.ValidateAsync(query, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var result = await mediator.Send(
            query,
            cancellationToken);

        var pagedSales = new PaginatedList<ListSalesItemResult>(
            result.Sales.ToList(),
            result.TotalCount,
            filter.PageNumber,
            filter.PageSize);

        return OkPaginated(pagedSales);
    }

    /// <summary>
    /// Creates a new sale
    /// </summary>
    /// <param name="request">The Sale request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var response = await mediator.Send(request, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<Guid>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = response
        });
    }

    /// <summary>
    /// Retrieves a sale by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSale(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Sale ID is required.");
        }

        var result = await mediator.Send(new GetSaleQuery(id), cancellationToken);

        return result.Match<IActionResult>(
            sale => Ok(sale),
            _ => NotFound("Sale not found."));
    }

    /// <summary>
    /// Partially updates a sale by its identifier.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSale(
        Guid id,
        [FromBody] UpdateSaleCommand request,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Sale ID is required.");
        }

        var command = request with { Id = id };
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var result = await mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            sale => Ok(sale),
            _ => NotFound("Sale not found."));
    }

    /// <summary>
    /// Permanently deletes a sale by its identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponseWithData<DeleteSaleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSale(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Sale ID is required.");
        }

        var result = await mediator.Send(new DeleteSaleCommand(id), cancellationToken);

        return result.Match<IActionResult>(
            sale => Ok(sale),
            _ => NotFound("Sale not found."));
    }

    /// <summary>
    /// Cancels a sale by its identifier.
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSale(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Sale ID is required.");
        }

        var result = await mediator.Send(new CancelSaleCommand(id), cancellationToken);

        return result.Match<IActionResult>(
            sale => Ok(sale),
            _ => NotFound("Sale not found."));
    }
}
