namespace AuctionService.Interfaces;

public interface IUnitOfWork
{
    IAuctionRepository AuctionRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
