using AuctionService;
using BiddingService.Entities;
using Grpc.Core;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
{
    public Auction? GetAuction(string auctionId)
    {
        logger.LogInformation("------ Calling Grpc auction service ------");

        var grpcAuctionConf = config["GrpcAuction"];
        if (grpcAuctionConf == null)
            throw new RpcException(new Status(StatusCode.FailedPrecondition,
                "GrpcAuction config has not been found"));

        var channel = GrpcChannel.ForAddress(grpcAuctionConf);
        var client = new GrpcAuction.GrpcAuctionClient(channel);

        var request = new GetAuctionRequest
        {
            AuctionId = auctionId
        };

        try
        {
            var reply = client.GetAuction(request);
            var auction = new Auction
            {
                ID = reply.Auction.Id,
                AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
                Seller = reply.Auction.Seller,
                ReservePrice = reply.Auction.ReservePrice
            };

            return auction;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not call Grpc server");
            return null;
        }
    }
}
