using System.Collections.Immutable;
using AuctionService.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Helpers;
using AuctionService.Interfaces;
using AutoFixture;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{
    private readonly Fixture fixture;
    private readonly Mock<IUnitOfWork> unitOfWork;
    private readonly Mock<IPublishEndpoint> publishEndpoint;
    private readonly IMapper mapper;
    private readonly AuctionsController auctionsController;

    public AuctionControllerTests()
    {
        fixture = new Fixture();
        unitOfWork = new Mock<IUnitOfWork>();
        publishEndpoint = new Mock<IPublishEndpoint>();

        var mockMapper = new MapperConfiguration(x =>
        {
            x.AddMaps(typeof(AutoMapperProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        mapper = new Mapper(mockMapper);
        auctionsController = new AuctionsController(unitOfWork.Object, mapper, publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = Utils.Helpers.GetClaimsPrincipal()
                }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        var auctions = fixture.CreateMany<AuctionDto>(10).ToList();
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionsAsync()).ReturnsAsync(auctions);

        var result = await auctionsController.GetAuctions();
        var actionResult = result.Result as OkObjectResult;
        var values = actionResult!.Value as IEnumerable<AuctionDto>;

        Assert.Equal(10, values!.Count());
        Assert.IsType<ActionResult<IEnumerable<AuctionDto>>>(result);
    }

    [Fact]
    public async Task GetAuction_WithValidId_ReturnsAuction()
    {
        var auction = fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = fixture.Build<Item>().Without(x => x.Auction).Create();
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionWithItemByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        var result = await auctionsController.GetAuction(auction.Id);
        var actionResult = result.Result as OkObjectResult;
        var value = actionResult!.Value as AuctionDto;

        Assert.Equal(auction.Item.Make, value!.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);
    }

    [Fact]
    public async Task GetAuction_WithInvalidId_ReturnsNotFound()
    {
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionWithItemByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        var result = await auctionsController.GetAuction(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidAuctionCreateDto_ReturnsCreatedAtAction()
    {
        var auction = fixture.Create<AuctionCreateDto>();
        unitOfWork.Setup(x => x.AuctionRepository.AddAuction(It.IsAny<Auction>()));
        unitOfWork.Setup(x => x.Complete()).ReturnsAsync(true);

        var result = await auctionsController.CreateAuction(auction);
        var actionResult = result.Result as CreatedAtActionResult;

        Assert.NotNull(actionResult);
        Assert.Equal("GetAuction", actionResult.ActionName);
        Assert.IsType<AuctionDto>(actionResult.Value);
    }

    [Fact]
    public async Task CreateAuction_FailedSave_Returns400BadRequest()
    {
        var auction = fixture.Create<AuctionCreateDto>();
        unitOfWork.Setup(x => x.AuctionRepository.AddAuction(It.IsAny<Auction>()));
        unitOfWork.Setup(x => x.Complete()).ReturnsAsync(false);

        var result = await auctionsController.CreateAuction(auction);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAuction_WithUpdateAuctionDto_ReturnsNoContent()
    {
        var auction = fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "test";
        var auctionToEdit = fixture.Create<AuctionEditDto>();
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionWithItemByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);
        unitOfWork.Setup(x => x.Complete()).ReturnsAsync(true);

        var result = await auctionsController.EditAuction(auctionToEdit, auction.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidUser_ReturnsUnauthorized()
    {
        var auction = fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = "otherUser";
        var auctionToEdit = fixture.Create<AuctionEditDto>();
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionWithItemByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        var result = await auctionsController.EditAuction(auctionToEdit, auction.Id);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsBadRequest()
    {
        var auction = fixture.Build<Auction>().Without(x => x.Item).Create();
        var auctionToEdit = fixture.Create<AuctionEditDto>();
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionWithItemByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        var result = await auctionsController.EditAuction(auctionToEdit, auction.Id);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithValidUser_ReturnsNoContent()
    {
        var auction = fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "test";
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);
        unitOfWork.Setup(x => x.Complete()).ReturnsAsync(true);

        var result = await auctionsController.DeleteAuction(auction.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidGuid_Returns404Response()
    {
        var auction = fixture.Build<Auction>().Without(x => x.Item).Create();
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(value: null);

        var result = await auctionsController.DeleteAuction(auction.Id);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_WithInvalidUser_ReturnsUnauthorized()
    {
        var auction = fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Seller = "otherUser";
        unitOfWork.Setup(x => x.AuctionRepository.GetAuctionByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(auction);

        var result = await auctionsController.DeleteAuction(auction.Id);

        Assert.IsType<UnauthorizedResult>(result);
    }
}
