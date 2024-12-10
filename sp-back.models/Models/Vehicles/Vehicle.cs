using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Auction;

namespace sp_back.models.Models.Vehicles;

public class Vehicle
{
    public Guid Id { get; set; }
    public virtual Auction.Auction? Auction { get; set; } = null!;
    public uint NumberOfDoors { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public DateTime ProductionDate { get; set; }
    public uint NumberOfSeats { get; set; }
    public double LoadCapacity { get; set; }
    public string Year => ProductionDate.Year.ToString();
    public VehicleType Type { get; set; }
    public double StartingPrice { get; set; }
    public double SoldByValue { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsSold => !String.IsNullOrEmpty(BuyerId);
    public string? BuyerId { get; set; }
    public Guid? AuctionId { get; set; }
    public string VIN { get; set; }
}