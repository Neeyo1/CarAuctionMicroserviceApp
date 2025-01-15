using AutoMapper;
using BiddingService.DTOs;
using BiddingService.Entities;

namespace BiddingService.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Bid, BidDto>();
    }
}
