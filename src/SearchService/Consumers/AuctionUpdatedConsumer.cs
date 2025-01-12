using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("Consuming AuctionUpdated: " + context.Message.Id);

        var item = mapper.Map<Item>(context.Message);

        var result = await DB.Update<Item>()
            .Match(x => x.ID == context.Message.Id)
            .ModifyOnly(x => new
            {
                x.Color,
                x.Make,
                x.Model,
                x.Year,
                x.Mileage
            }, item)
            .ExecuteAsync();

        if (!result.IsAcknowledged)
            throw new MessageException(typeof(AuctionUpdated), 
                "Problem occured while updating auction in search database");
    }
}
