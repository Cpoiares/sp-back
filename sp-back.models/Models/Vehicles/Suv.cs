using sp_back_api.DTOs.Responses;
using sp_back.models.Enums;
using sp_back.models.Exceptions;

namespace sp_back.models.Models.Vehicles;

public class Suv : Vehicle 
{
    public uint NumberOfSeats { get; set; }
    public override VehicleResponse GetVehicleResponses()
    {
        return new VehicleResponse()
        {
            Id = Id,
            Vin = VIN,
            Make = Make,
            Model = Model,
            NumberOfSeats = NumberOfSeats,
            Available = IsAvailable,
            VehicleType = VehicleType.Suv,
            Sold = IsSold,
            StartingPrice = StartingPrice,
            AuctionId = Auction?.Id ?? null,
            BidderId = IsSold ? Auction.GetHighestBidderForVehicle(Id) : null
        };    
    }
}