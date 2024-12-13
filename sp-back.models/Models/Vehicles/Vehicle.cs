using sp_back_api.DTOs.Responses;
using sp_back.models.Enums;


namespace sp_back.models.Models.Vehicles;

public abstract class Vehicle
{
    public Guid Id { get; set; }
    public virtual Auction.Auction? Auction { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public DateTime ProductionDate { get; set; }
    public string Year => ProductionDate.Year.ToString();
    public VehicleType Type { get; set; } 
    public double StartingPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsSold => !String.IsNullOrEmpty(BuyerId);
    public string? BuyerId { get; set; }
    public Guid? AuctionId { get; set; }
    public string VIN { get; set; }

    public abstract VehicleResponse GetVehicleResponses();

}
