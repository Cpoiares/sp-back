using sp_back.models.DTOs.Responses;
using sp_back.models.Enums;

namespace sp_back.models.Models.Vehicles;

public class Suv : Vehicle 
{
    public Suv(string make, string model, DateTime productionDate, double startingPrice, string vin, uint numberOfSeats) : base(make, model, productionDate, startingPrice, vin)
    {
        NumberOfSeats = numberOfSeats;
        Type = VehicleType.Suv;
    }

    public uint NumberOfSeats { get; set; }
    public override VehicleResponse GetVehicleResponses()
    {
        return new GetVehicleSuvResponse()
        {
            Id = Id,
            Vin = Vin,
            Make = Make,
            Model = Model,
            NumberOfSeats = NumberOfSeats,
            Available = IsAvailable,
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