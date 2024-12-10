using sp_back_api.DTOs.Responses;
using sp_back.models.Models.Auction;

namespace sp_back_api.Extensions;

public static class AuctionResponseHandler
{
    public static AuctionResponse BuildResponse(Auction auction)
    {
        return new AuctionResponse()
        {
            AuctionName = auction.Name,
            AuctionId = auction.Id,
            Vehicles = auction.Vehicles.Select(v =>  new AuctionVehicles()
            {
                VIN = v.VIN,
                Name = $"{v.Make}{v.Model}",
                CurrentBid = auction.GetHighestBidForVehicle(v.Id).Amount,
                BidderId = auction.GetHighestBidderForVehicle(v.Id)
            }).ToList(),
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            Status = auction.Status,
        };
    }
}