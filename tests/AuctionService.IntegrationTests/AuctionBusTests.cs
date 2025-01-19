using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

public class AuctionBusTests(CustomWebAppFactory factory) :
    IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient httpClient = factory.CreateClient();
    private readonly ITestHarness testHarness = factory.Services.GetTestHarness();

    [Fact]
    public async Task CreateAuction_WithValidObject_ShouldPublishAuctionCreated()
    {
        var auction = GetAuctionToCreate();
        httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        var response = await httpClient.PostAsJsonAsync("api/auctions", auction);

        response.EnsureSuccessStatusCode();
        Assert.True(await testHarness.Published.Any<AuctionCreated>());
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
