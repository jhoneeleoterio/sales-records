namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Represents the error contract exposed by the API.
/// </summary>
public sealed class ApiErrorResponse
{
    public string Type { get; init; } = string.Empty;
    public string Error { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;

    public static ApiErrorResponse Validation(IEnumerable<string> details) => new()
    {
        Type = "ValidationError",
        Error = "Invalid input data.",
        Detail = string.Join(" ", details)
    };

    public static ApiErrorResponse NotFound(string detail) => new()
    {
        Type = "ResourceNotFound",
        Error = "Resource not found.",
        Detail = detail
    };
}
