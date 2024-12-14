using sp_back.models.DTOs.Responses;
using sp_back.models.Enums;

namespace sp_back.models.Models.Vehicles;

public class Truck : Vehicle
{
    public Truck(string make, string model, DateTime productionDate, double startingPrice, string vin, double loadCapacity) : base(make, model, productionDate, startingPrice, vin)
    {
        LoadCapacity = loadCapacity;
        Type = VehicleType.Truck;
    }

    public double LoadCapacity { get; set; }

    public override VehicleResponse GetVehicleResponses()
    {
        return new GetVehicleTruckResponse()
        {
            Id = Id,
            Vin = Vin,
            Make = Make,
            Model = Model,
            LoadCapacity = LoadCapacity,
            Available = IsAvailable,
            VehicleType = VehicleType.Truck,
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
