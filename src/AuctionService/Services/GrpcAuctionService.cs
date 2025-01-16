using AuctionService.Data;
using AuctionService.Interfaces;
using Grpc.Core;

namespace AuctionService.Services;

public class GrpcAuctionService(IUnitOfWork unitOfWork) : GrpcAuction.GrpcAuctionBase
{
    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, 
        ServerCallContext context)
    {
        Console.WriteLine("------ Received Grpc request for auction ------");

        var auction = await unitOfWork.AuctionRepository.GetAuctionWithItemByIdAsync(
            Guid.Parse(request.AuctionId));
        if (auction == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Not found"));

        var response = new GrpcAuctionResponse
        {
            Auction = new GrpcAuctionModel
            {
                AuctionEnd = auction.AuctionEnd.ToString(),
                Id = auction.Id.ToString(),
                ReservePrice = auction.ReservePrice,
                Seller = auction.Seller
            }
        };

        return response;
    }
}
