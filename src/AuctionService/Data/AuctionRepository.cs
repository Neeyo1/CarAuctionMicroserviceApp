using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionRepository(AuctionDbContext context, IMapper mapper) : IAuctionRepository
{
    public void AddAuction(Auction auction)
    {
        context.Auctions.Add(auction);
    }

    public void DeleteAuction(Auction auction)
    {
        context.Auctions.Remove(auction);
    }

    public async Task<IEnumerable<AuctionDto>> GetAuctionsAsync()
    {
        return await context.Auctions
            .Include(x => x.Item)
            .ProjectTo<AuctionDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }
    public async Task<Auction?> GetAuctionByIdAsync(Guid auctionId)
    {
        return await context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == auctionId);
    }
}
