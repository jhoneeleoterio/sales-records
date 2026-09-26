using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FluentValidation.Results;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected int GetCurrentUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new NullReferenceException());

    protected string GetCurrentUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? throw new NullReferenceException();

    protected IActionResult Ok<T>(T data) =>
            base.Ok(new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult Created<T>(string routeName, object routeValues, T data) =>
        base.CreatedAtRoute(routeName, routeValues, new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult BadRequest(string message) =>
        base.BadRequest(ApiErrorResponse.Validation([message]));

    protected IActionResult BadRequest(IEnumerable<ValidationFailure> failures) =>
        base.BadRequest(ApiErrorResponse.Validation(failures.Select(failure => failure.ErrorMessage)));

    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(ApiErrorResponse.NotFound(message));

    protected IActionResult OkPaginated<T>(PaginatedList<T> pagedList) =>
            base.Ok(new PaginatedResponse<T>
            {
                Data = pagedList,
                CurrentPage = pagedList.CurrentPage,
                PageSize = pagedList.PageSize,
                TotalPages = pagedList.TotalPages,
                TotalCount = pagedList.TotalCount,
                Success = true
            });
}
