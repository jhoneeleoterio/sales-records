using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public sealed record SaleFilter(
    int? Number = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    Guid? ProductId = null,
    SaleStatus? Status = null,
    decimal? MinTotalAmount = null,
    decimal? MaxTotalAmount = null,
    DateTime? MinCreatedAt = null,
    DateTime? MaxCreatedAt = null,
    string? Order = null);

public static class SaleOrdering
{
    private static readonly HashSet<string> SupportedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "number",
        "createdAt",
        "totalAmount"
    };

    public static bool IsValid(string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
        {
            return true;
        }

        var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in order.Split(',', StringSplitOptions.TrimEntries))
        {
            var tokens = segment.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length is < 1 or > 2 ||
                !SupportedFields.Contains(tokens[0]) ||
                !fields.Add(tokens[0]) ||
                (tokens.Length == 2 &&
                 !tokens[1].Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                 !tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        return true;
    }
}
