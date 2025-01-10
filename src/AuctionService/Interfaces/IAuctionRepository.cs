using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Interfaces;

public interface IAuctionRepository
{
    void AddAuction(Auction auction);
    void DeleteAuction(Auction auction);
    Task<IEnumerable<AuctionDto>> GetAuctionsAsync();
    Task<Auction?> GetAuctionByIdAsync(Guid auctionId);
}
