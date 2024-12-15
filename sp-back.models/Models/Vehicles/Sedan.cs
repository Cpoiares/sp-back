using sp_back.models.DTOs.Responses;
using sp_back.models.Enums;

namespace sp_back.models.Models.Vehicles;

public class Sedan : Vehicle
{
    public Sedan(string manufacturer, string model, DateTime productionDate, double startingPrice, string vin, uint numberOfDoors) : base(manufacturer, model, productionDate, startingPrice, vin)
    {
        NumberOfDoors = numberOfDoors;
        Type = VehicleType.Sedan;
    }

    public uint NumberOfDoors { get; set; }
    public override VehicleResponse GetVehicleResponses()
    {
        return new GetVehicleSedanHatchbackResponse()
        {
            Id = Id,
            Vin = Vin,
            Manufacturer = Manufacturer,
            Model = Model,
            NumberOfDoors = NumberOfDoors,
            Available = IsAvailable,
            VehicleType = VehicleType.Sedan,
            Sold = IsSold,
            StartingPrice = StartingPrice,
            AuctionId = Auction?.Id,
            BidderId = GetBidderId()
        };
    }
    
    private string? GetBidderId()
    {
        if (!IsSold || Auction == null)
            return null;
        
        try
        {
            return Auction.GetHighestBidderForVehicle(Id);
        }
        catch
        {
            return null;
        }
    }
}