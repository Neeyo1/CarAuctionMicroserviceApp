using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Auction, AuctionDto>()
            .IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        CreateMap<AuctionCreateDto, Auction>()
            .ForMember(x => x.Item, y => y.MapFrom(z => z));
        CreateMap<AuctionCreateDto, Item>();
        CreateMap<AuctionDto, AuctionCreated>();
        CreateMap<Auction, AuctionUpdated>()
            .IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionUpdated>();

        CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s));
        CreateMap<string, TimeOnly>().ConvertUsing(s => TimeOnly.Parse(s));
        CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
        CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue 
            ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
    }
}
