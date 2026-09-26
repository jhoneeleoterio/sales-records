using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Ambev.DeveloperEvaluation.Functional.TestData;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Features.Sales;

/// <summary>
/// Verifies the HTTP contract for retrieving a sale by identifier.
/// </summary>
public class GetSaleTests : IClassFixture<FunctionalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GetSaleTests(FunctionalWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>
    /// Creates a sale and verifies that its header data and items are returned by the detail endpoint.
    /// </summary>
    [Fact]
    public async Task GetSale_WithExistingId_ShouldReturnOk()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/api/Sale", command);
        var createdContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<Guid>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdContent);

        var response = await _client.GetAsync($"/api/Sale/{createdContent.Data}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<ApiResponseWithData<GetSaleResult>>();

        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotNull(content.Data);
        Assert.Equal(createdContent.Data, content.Data.Id);
        Assert.Equal(command.CustomerId, content.Data.CustomerId);
        Assert.Equal(command.Items.Count, content.Data.Items.Count);
    }

    /// <summary>
    /// Requests an identifier that does not exist and verifies that the API returns 404.
    /// </summary>
    [Fact]
    public async Task GetSale_WithUnknownId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/Sale/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
