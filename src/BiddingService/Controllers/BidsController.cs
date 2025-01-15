using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers;

public class BidsController(IMapper mapper) : BaseApiController
{
    [HttpGet("{auctionId}")]
    public async Task<ActionResult<IEnumerable<BidDto>>> GetBidsForAuction(string auctionId)
    {
        var bids = await DB.Find<Bid>()
            .Match(x => x.AuctionId == auctionId)
            .Sort(x => x.Descending(y => y.BidTime))
            .ExecuteAsync();

        return Ok(bids.Select(mapper.Map<BidDto>).ToList());
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var identity = User.Identity;
        if (identity == null)
            return BadRequest("Failed to get identity of a user");

        var auction = await DB.Find<Auction>().OneAsync(auctionId);
        if (auction == null)
        {
            //Check in service if auction exist
            return NotFound();
        }
        if (auction.Seller == identity.Name)
            return BadRequest("You cannot bid on your own auction");

        var bid = new Bid
        {
            Amount = amount,
            AuctionId = auctionId,
            Bidder = identity.Name
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highestBid = await DB.Find<Bid>()
                .Match(x => x.AuctionId == auctionId)
                .Sort(x => x.Descending(y => y.Amount))
                .ExecuteFirstAsync();
            if (highestBid != null && amount > highestBid.Amount || highestBid == null)
            {
                bid.BidStatus = amount > auction.ReservePrice
                    ? BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
            }
            else if (highestBid != null && amount <= highestBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }

        await DB.SaveAsync(bid);

        return Ok(mapper.Map<BidDto>(bid));
    }
}
