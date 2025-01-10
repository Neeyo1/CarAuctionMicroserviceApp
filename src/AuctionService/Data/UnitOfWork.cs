using AuctionService.Interfaces;

namespace AuctionService.Data;

public class UnitOfWork(AuctionDbContext context, IAuctionRepository auctionRepository) : IUnitOfWork
{
    public IAuctionRepository AuctionRepository => auctionRepository;

    public async Task<bool> Complete()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
