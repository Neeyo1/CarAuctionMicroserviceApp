using AutoMapper;
using Contracts;
using SearchService.Entities;

namespace AuctionService.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AuctionCreated, Item>();
    }
}
