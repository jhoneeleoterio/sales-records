using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Ambev.DeveloperEvaluation.Functional.TestData;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Features.Sales;

/// <summary>
/// Verifies the HTTP contract for partially updating sales.
/// </summary>
public class UpdateSaleTests : IClassFixture<FunctionalWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UpdateSaleTests(FunctionalWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    /// <summary>
    /// Updates the complete customer external identity and verifies that fields omitted from the request are preserved.
    /// </summary>
    [Fact]
    public async Task UpdateSale_WithCustomerExternalIdentity_ShouldOnlyUpdateTheProvidedField()
    {
        var createCommand = CreateSaleFunctionalTestData.GenerateValidCommand();
        var createResponse = await _client.PostAsJsonAsync("/api/Sale", createCommand);
        var createdContent = await createResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<Guid>>();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createdContent);

        var updatedCustomerId = Guid.NewGuid();
        var request = new UpdateSaleCommand(updatedCustomerId, "Cliente Atualizado");
        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/Sale/{createdContent.Data}",
            request);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updateContent = await updateResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<UpdateSaleResult>>();

        Assert.NotNull(updateContent);
        Assert.True(updateContent.Success);
        Assert.NotNull(updateContent.Data);
        Assert.Equal(createdContent.Data, updateContent.Data.Id);
        Assert.NotNull(updateContent.Data.UpdatedAt);

        var getResponse = await _client.GetAsync($"/api/Sale/{createdContent.Data}");
        var saleContent = await getResponse.Content
            .ReadFromJsonAsync<ApiResponseWithData<GetSaleResult>>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(saleContent);
        Assert.NotNull(saleContent.Data);
        Assert.Equal(updatedCustomerId, saleContent.Data.CustomerId);
        Assert.Equal("Cliente Atualizado", saleContent.Data.CustomerName);
        Assert.Equal(createCommand.BranchId, saleContent.Data.BranchId);
        Assert.Equal(createCommand.Items.Count, saleContent.Data.Items.Count);
    }

    /// <summary>
    /// Attempts to update an unknown sale and verifies that the API returns 404.
    /// </summary>
    [Fact]
    public async Task UpdateSale_WithUnknownId_ShouldReturnNotFound()
    {
        var request = new UpdateSaleCommand(Guid.NewGuid(), "Cliente Atualizado");

        var response = await _client.PatchAsJsonAsync($"/api/Sale/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Sends only part of a customer external identity and verifies that the request is rejected.
    /// </summary>
    [Fact]
    public async Task UpdateSale_WithOnlyCustomerId_ShouldReturnBadRequest()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/Sale/{Guid.NewGuid()}",
            new UpdateSaleCommand(CustomerId: Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("ValidationError", error.Type);
    }
}
