using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

[Collection("Shared collection")]
public class AuctionControllerTests(CustomWebAppFactory factory) : IAsyncLifetime
{
    private readonly HttpClient httpClient = factory.CreateClient();
    private const string AUCTION_ID = "afbee524-5972-4075-8800-7d1f9d7b0a0c";

    [Fact]
    public async Task GetAucions_ShouldReturn3Auctions()
    {
        var response = await httpClient.GetFromJsonAsync<IEnumerable<AuctionDto>>("api/auctions");

        Assert.NotNull(response);
        Assert.Equal(3, response.Count());
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
    {
        var response = await httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{AUCTION_ID}");

        Assert.NotNull(response);
        Assert.Equal("GT", response.Model);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_ShouldReturnBadRequest()
    {
        var response = await httpClient.GetAsync("api/auctions/NotAGuid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithNoAuth_ShouldReturnUnauthorized()
    {
        var auction = GetAuctionToCreate();

        var response = await httpClient.PostAsJsonAsync("api/auctions", auction);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_ShouldReturnCreated()
    {
        var auction = GetAuctionToCreate();
        httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        var response = await httpClient.PostAsJsonAsync("api/auctions", auction);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();
        Assert.NotNull(createdAuction);
        Assert.Equal("bob", createdAuction.Seller);
    }

    [Fact]
    public async Task CreateAuction_WithInvalidCreateAuctionDto_ShouldReturnBadRequest()
    {
        var auction = GetAuctionToCreate();
        auction.ImageUrl = "";
        httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        var response = await httpClient.PostAsJsonAsync("api/auctions", auction);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndUser_ShouldReturnNoContent()
    {
        var auction = GetAuctionToUpdate();
        httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        var response = await httpClient.PutAsJsonAsync($"api/auctions/{AUCTION_ID}", auction);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_WithValidUpdateDtoAndInvalidUser_ShouldReturnUnauthorized()
    {
        var auction = GetAuctionToUpdate();
        httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("tom"));

        var response = await httpClient.PutAsJsonAsync($"api/auctions/{AUCTION_ID}", auction);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(context);

        return Task.CompletedTask;
    }

    private static AuctionCreateDto GetAuctionToCreate()
    {
        return new AuctionCreateDto
        {
            Make = "TestMake",
            Model = "TestModel",
            Color = "TestColor",
            ImageUrl = "TestUrl",
            Mileage = 10,
            Year = 2000,
            ReservePrice = 1000
        };
    }

    private static AuctionEditDto GetAuctionToUpdate()
    {
        return new AuctionEditDto
        {
            Make = "TestMakeEdited"
        };
    }
}
