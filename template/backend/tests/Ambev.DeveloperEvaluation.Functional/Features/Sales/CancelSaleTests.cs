using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Ambev.DeveloperEvaluation.Functional.TestData;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Features.Sales;

/// <summary>
/// Verifies the HTTP contract for cancelling sales.
/// </summary>
public class CancelSaleTests : IClassFixture<FunctionalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CancelSaleTests(FunctionalWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>
    /// Creates a sale, cancels it, and verifies the cancelled status and update timestamp in the response.
    /// </summary>
    [Fact]
    public async Task CancelSale_WithExistingId_ShouldReturnCancelledSale()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/api/Sale", command);
        var createdContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<Guid>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdContent);

        var response = await _client.PatchAsync($"/api/Sale/{createdContent.Data}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<ApiResponseWithData<CancelSaleResult>>();

        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotNull(content.Data);
        Assert.Equal(createdContent.Data, content.Data.Id);
        Assert.Equal(SaleStatus.Cancelled, content.Data.Status);
        Assert.NotNull(content.Data.UpdatedAt);
    }

    /// <summary>
    /// Attempts to cancel an unknown sale and verifies that the API returns 404.
    /// </summary>
    [Fact]
    public async Task CancelSale_WithUnknownId_ShouldReturnNotFound()
    {
        var response = await _client.PatchAsync($"/api/Sale/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
