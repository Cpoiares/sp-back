using sp_back_api.DTOs.Responses;
using sp_back.models.Enums;

namespace sp_back.models.Models.Vehicles;

public class Sedan : Vehicle
{
    public uint NumberOfDoors { get; set; }
    public override VehicleResponse GetVehicleResponses()
    {
        return new VehicleResponse()
        {
            Id = Id,
            Vin = VIN,
            Make = Make,
            Model = Model,
            NumberOfDoors = NumberOfDoors,
            Available = IsAvailable,
            VehicleType = VehicleType.Sedan,
            Sold = IsSold,
            StartingPrice = StartingPrice,
            AuctionId = Auction?.Id ?? null,
            BidderId = IsSold ? Auction.GetHighestBidderForVehicle(Id) : null
        };
    }
}