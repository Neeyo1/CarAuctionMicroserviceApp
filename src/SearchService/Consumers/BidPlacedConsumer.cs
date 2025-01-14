using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("Consuming BidPlaced: " + context.Message.AuctionId);

        var auction = await DB.Find<Item>()
            .OneAsync(context.Message.AuctionId!);
        if (auction == null)
            throw new MessageException(typeof(BidPlaced), 
                "Problem occured while saving highest bid in search database");

        if (context.Message.BidStatus!.Contains("Accepted")
            && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}
