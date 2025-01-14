using AuctionService.Entities;
using AuctionService.Interfaces;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer(IUnitOfWork unitOfWork) : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("Consuming AuctionFinished: " + context.Message.AuctionId);

        var auction = await unitOfWork.AuctionRepository.GetAuctionByIdAsync(Guid.Parse(context.Message.AuctionId!));
        if (auction == null)
            throw new MessageException(typeof(AuctionFinished), 
                "Problem occured while finishing auction in auction database");

        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = context.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice
            ? Status.Finished : Status.ReserveNotMet;

        await unitOfWork.Complete();
    }
}
