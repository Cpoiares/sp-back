using sp_back.models.DTOs.Responses;
using sp_back.models.Models.Auction;

namespace sp_back_api.Extensions;

public static class AuctionResponseHandler
{
    public static AuctionResponse BuildResponse(Auction auction)
    {
        return new AuctionResponse()
        {
            AuctionId = auction.Id,
            Vehicles = auction.Vehicles.Select(v =>  new AuctionVehicles()
            {
                Vin = v.Vin,
                Name = $"{v.Manufacturer} {v.Model}",
                VehicleStartingPrice = v.StartingPrice,
                WinningBid = string.IsNullOrEmpty(auction.GetHighestBidderForVehicle(v.Id)) ? null : auction.GetHighestBidForVehicle(v.Id).Amount,
                BidderId = auction.GetHighestBidderForVehicle(v.Id)
            }).ToList(),
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            Status = auction.Status,
            IsCollectiveAuction = auction.IsCollectiveAuction,
        };
    }
    
    public static StartAuctionResponse BuildStartResponse(Auction auction)
    {
        return new StartAuctionResponse()
        {
            AuctionId = auction.Id,
            Vehicles = auction.Vehicles.Select(v =>  new AuctionVehicles()
            {
                Vin = v.Vin,
                Name = $"{v.Manufacturer}{v.Model}",
                VehicleStartingPrice = v.StartingPrice,
                WinningBid =  string.IsNullOrEmpty(auction.GetHighestBidderForVehicle(v.Id)) ? null : auction.GetHighestBidForVehicle(v.Id).Amount,
                BidderId = auction.GetHighestBidderForVehicle(v.Id)
            }).ToList(),
            StartDate = auction.StartTime ?? DateTime.UtcNow,
            
        };
    }
    
    public static CloseAuctionResponse BuildCloseResponse(Auction auction)
    {
        return new CloseAuctionResponse()
        {
            AuctionId = auction.Id,
            Vehicles = auction.Vehicles.Select(v =>  new AuctionVehicles()
            {
                Vin = v.Vin,
                Name = $"{v.Manufacturer}{v.Model}",
                VehicleStartingPrice = v.StartingPrice,
                WinningBid = string.IsNullOrEmpty(auction.GetHighestBidderForVehicle(v.Id)) ? null : auction.GetHighestBidForVehicle(v.Id).Amount,
                BidderId = auction.GetHighestBidderForVehicle(v.Id)
            }).ToList(),
            EndDate = auction.EndTime ?? DateTime.UtcNow,
        };
    }
    
    public static AuctionBidHistoryResponse BuildBidHistoryResponse(Auction auction)
    {
        return new AuctionBidHistoryResponse()
        {
            AuctionId = auction.Id,
            IsCollectiveAuction = auction.IsCollectiveAuction,
            Bids = auction.Bids.OrderBy(a => a.BidTime).Select(b =>
            {
                var vehicleId = auction.IsCollectiveAuction ? auction.Vehicles[0].Id : b.VehicleId;
                var highestBid = auction.GetHighestBinInHistoryForVehicle(vehicleId);
            
                return new AuctionBids()
                {
                    BidAmount = b.Amount,
                    BidderId = b.BidderId,
                    BidTime = b.BidTime,
                    VehicleId = auction.IsCollectiveAuction ? null : b.VehicleId,
                    IsWinnerBid = highestBid != null && highestBid.Id == b.Id
                };
            }).ToList()
        };
    }
}