using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        Console.WriteLine("Consuming AuctionFinished: " + context.Message.AuctionId);

        var auction = await DB.Find<Item>()
            .OneAsync(context.Message.AuctionId!);
        if (auction == null)
            throw new MessageException(typeof(AuctionFinished), 
                "Problem occured while finishing auction in auction database");

        if (context.Message.ItemSold && context.Message.Amount != null)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = (int)context.Message.Amount;
        }

        auction.Status = "Finished";

        await auction.SaveAsync();
    }
}
