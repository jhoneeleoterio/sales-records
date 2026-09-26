using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Ambev.DeveloperEvaluation.Functional.TestData;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Features.Sales;

/// <summary>
/// Verifies the HTTP contract for paginated sales listing.
/// </summary>
public class ListSalesTests : IClassFixture<FunctionalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ListSalesTests(FunctionalWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>
    /// Creates a sale and verifies that the requested page returns it together with pagination metadata.
    /// </summary>
    [Fact]
    public async Task GetSales_ShouldReturnPagedSales()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/api/Sale", command);
        var createdContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<Guid>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdContent);

        var response = await _client.GetAsync(
            $"/api/Sale?_page=1&_size=10&customerId={command.CustomerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<PaginatedResponse<ListSalesItemResult>>();

        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotNull(content.Data);
        Assert.Contains(content.Data, sale => sale.Id == createdContent.Data);
        Assert.Equal(1, content.CurrentPage);
        Assert.Equal(10, content.PageSize);
    }

    /// <summary>
    /// Sends explicit pagination aliases and verifies that the query model binds them to the response metadata.
    /// </summary>
    [Fact]
    public async Task GetSales_WithExplicitPageAndSize_ShouldBindQueryAliases()
    {
        var response = await _client.GetAsync("/api/Sale?_page=2&_size=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<PaginatedResponse<ListSalesItemResult>>();

        Assert.NotNull(content);
        Assert.Equal(2, content.CurrentPage);
        Assert.Equal(1, content.PageSize);
    }

    /// <summary>
    /// Creates a sale and verifies that its product identifier can be used as an HTTP list filter.
    /// </summary>
    [Fact]
    public async Task GetSales_WithProductIdFilter_ShouldReturnMatchingSale()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/api/Sale", command);
        var createdContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<Guid>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdContent);

        var productId = command.Items[0].ProductId;
        var response = await _client.GetAsync($"/api/Sale?productId={productId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<PaginatedResponse<ListSalesItemResult>>();

        Assert.NotNull(content);
        Assert.NotNull(content.Data);
        Assert.Contains(content.Data, sale => sale.Id == createdContent.Data);
    }

    /// <summary>
    /// Sends invalid pagination and range values and verifies that the API returns the standard validation error contract.
    /// </summary>
    [Fact]
    public async Task GetSales_WithInvalidFilters_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync(
            "/api/Sale?_size=101&_minTotalAmount=100&_maxTotalAmount=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
        Assert.Equal("Invalid input data.", error.Error);
    }

    /// <summary>
    /// Sends an explicit ascending order with a secondary criterion and verifies that the request is accepted.
    /// </summary>
    [Fact]
    public async Task GetSales_WithMultipleOrderCriteria_ShouldReturnOk()
    {
        var response = await _client.GetAsync(
            "/api/Sale?_order=createdAt%20asc,totalAmount%20desc");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
