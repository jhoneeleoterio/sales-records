using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Ambev.DeveloperEvaluation.Functional.TestData;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Features.Sales;

/// <summary>
/// Verifies the HTTP contract for creating sales.
/// </summary>
public class CreateSaleTests : IClassFixture<FunctionalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CreateSaleTests(FunctionalWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>
    /// Posts a valid sale request and verifies that the API creates it and returns its identifier.
    /// </summary>
    [Fact]
    public async Task CreateSale_WithValidRequest_ShouldReturnCreated()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();

        var response = await _client.PostAsJsonAsync("/api/Sale", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<ApiResponseWithData<Guid>>();

        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotEqual(Guid.Empty, content.Data);
    }

    /// <summary>
    /// Posts a sale with an empty customer identifier and verifies that request validation returns 400.
    /// </summary>
    [Fact]
    public async Task CreateSale_WithEmptyCustomerId_ShouldReturnBadRequest()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();
        command.CustomerId = Guid.Empty;

        var response = await _client.PostAsJsonAsync("/api/Sale", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
        Assert.Equal("Invalid input data.", error.Error);
    }

    /// <summary>
    /// Posts an invalid item quantity and verifies that the request fails before the domain handler is executed.
    /// </summary>
    [Fact]
    public async Task CreateSale_WithInvalidItemQuantity_ShouldReturnBadRequest()
    {
        var command = CreateSaleFunctionalTestData.GenerateValidCommand();
        command.Items[0].Quantity = 21;

        var response = await _client.PostAsJsonAsync("/api/Sale", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
    }
}
