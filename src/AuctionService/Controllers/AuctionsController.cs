using API.Controllers;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Interfaces;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

public class AuctionsController(IUnitOfWork unitOfWork, IMapper mapper,
    IPublishEndpoint publishEndpoint) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuctionDto>>> GetAuctions()
    {
        var auctions = await unitOfWork.AuctionRepository.GetAuctionsAsync();
        
        return Ok(auctions);
    }

    [HttpGet("{auctionId}")]
    public async Task<ActionResult<AuctionDto>> GetAuction(Guid auctionId)
    {
        var auction = await unitOfWork.AuctionRepository.GetAuctionByIdAsync(auctionId);
        if (auction == null) return NotFound();

        return Ok(mapper.Map<AuctionDto>(auction));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(AuctionCreateDto auctionCreateDto)
    {
        var identity = User.Identity;
        if (identity == null)
            return BadRequest("Failed to get identity of a user");

        var auction = mapper.Map<Auction>(auctionCreateDto);
        auction.Seller = identity.Name;

        unitOfWork.AuctionRepository.AddAuction(auction);

        var newAuction = mapper.Map<AuctionDto>(auction);

        await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));

        if (await unitOfWork.Complete())
            return CreatedAtAction(nameof(GetAuction), new {auctionId = auction.Id}, newAuction);
            
        return BadRequest("Failed to create auction");
    }

    [Authorize]
    [HttpPut("{auctionId}")]
    public async Task<ActionResult> EditAuction(AuctionEditDto auctionEditDto, Guid auctionId)
    {
        var identity = User.Identity;
        if (identity == null)
            return BadRequest("Failed to get identity of a user");

        var auction = await unitOfWork.AuctionRepository.GetAuctionByIdAsync(auctionId);
        if (auction == null) return BadRequest("Failed to find auction");

        if (auction.Seller != identity.Name)
            return Unauthorized();

        auction.Item.Make = auctionEditDto.Make ?? auction.Item.Make;
        auction.Item.Model = auctionEditDto.Model ?? auction.Item.Model;
        auction.Item.Color = auctionEditDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionEditDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionEditDto.Year ?? auction.Item.Year;

        await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

        if (await unitOfWork.Complete()) return NoContent();
        return BadRequest("Failed to edit auction");
    }

    [Authorize]
    [HttpDelete("{auctionId}")]
    public async Task<ActionResult> DeleteAuction(Guid auctionId)
    {
        var identity = User.Identity;
        if (identity == null)
            return BadRequest("Failed to get identity of a user");
            
        var auction = await unitOfWork.AuctionRepository.GetAuctionByIdAsync(auctionId);
        if (auction == null) return BadRequest("Failed to find auction");

        if (auction.Seller != identity.Name)
            return Unauthorized();

        unitOfWork.AuctionRepository.DeleteAuction(auction);

        await publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});

        if (await unitOfWork.Complete()) return NoContent();
        return BadRequest("Failed to delete auction");
    }
}
