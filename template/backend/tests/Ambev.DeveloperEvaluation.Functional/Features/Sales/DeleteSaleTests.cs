using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Ambev.DeveloperEvaluation.Functional.TestData;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Features.Sales;

/// <summary>
/// Verifies the HTTP contract for permanently deleting sales.
/// </summary>
public class DeleteSaleTests : IClassFixture<FunctionalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DeleteSaleTests(FunctionalWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>
    /// Creates a sale, deletes it, and verifies that a subsequent detail request returns 404.
    /// </summary>
    [Fact]
    public async Task DeleteSale_WithExistingId_ShouldRemoveSale()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/api/Sale", command);
        var createdContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<Guid>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdContent);

        var deleteResponse = await _client.DeleteAsync($"/api/Sale/{createdContent.Data}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var deleteContent = await deleteResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<DeleteSaleResult>>();

        Assert.NotNull(deleteContent);
        Assert.True(deleteContent.Success);
        Assert.NotNull(deleteContent.Data);
        Assert.Equal(createdContent.Data, deleteContent.Data.Id);

        var getResponse = await _client.GetAsync($"/api/Sale/{createdContent.Data}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    /// <summary>
    /// Attempts to delete an unknown sale and verifies that the API returns 404.
    /// </summary>
    [Fact]
    public async Task DeleteSale_WithUnknownId_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync($"/api/Sale/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
