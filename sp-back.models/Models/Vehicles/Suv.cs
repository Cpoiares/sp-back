using sp_back.models.DTOs.Responses;
using sp_back.models.Enums;

namespace sp_back.models.Models.Vehicles;

public class Suv : Vehicle 
{
    public Suv(string manufacturer, string model, DateTime productionDate, double startingPrice, string vin, uint numberOfSeats) : base(manufacturer, model, productionDate, startingPrice, vin)
    {
        NumberOfSeats = numberOfSeats;
        Type = VehicleType.Suv;
    }

    private uint _numberOfSeats { get; set; }
    
    public uint NumberOfSeats
    {
        get => _numberOfSeats;
        set => _numberOfSeats = value;
    }
    public override VehicleResponse GetVehicleResponses()
    {
        return new GetVehicleSuvResponse()
        {
            Id = Id,
            Vin = Vin,
            Manufacturer = Manufacturer,
            Model = Model,
            NumberOfSeats = NumberOfSeats,
            Available = IsAvailable,
            ProductionDate = ProductionDate,
            VehicleType = VehicleType.Suv,
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