using AuctionService.Interfaces;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class BidPlacedConsumer(IUnitOfWork unitOfWork) : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("Consuming BidPlaced: " + context.Message.AuctionId);

        var auction = await unitOfWork.AuctionRepository.GetAuctionByIdAsync(Guid.Parse(context.Message.AuctionId!));
        if (auction == null)
            throw new MessageException(typeof(BidPlaced), 
                "Problem occured while saving highest bid in auction database");

        if (auction.CurrentHighBid == null 
            || context.Message.BidStatus!.Contains("Accepted")
            && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await unitOfWork.Complete();
        }
    }
}
