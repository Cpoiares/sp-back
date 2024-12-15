using sp_back.models.DTOs.Responses;
using sp_back.models.Enums;


namespace sp_back.models.Models.Vehicles;

public abstract class Vehicle
{
    protected Vehicle(string manufacturer, string model, DateTime productionDate, double startingPrice, string vin)
    {
        Manufacturer = manufacturer;
        Model = model;
        ProductionDate = productionDate;
        StartingPrice = startingPrice;
        Vin = vin;
        IsDeleted = false;
    }


    public int Id { get; set; }
    public virtual Auction.Auction? Auction { get; init; }
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public DateTime ProductionDate { get; set; }
    public string Year => ProductionDate.Year.ToString();
    public VehicleType Type { get; init; } 
    public double StartingPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsSold => !string.IsNullOrEmpty(BuyerId);
    public string? BuyerId { get; set; }
    public int? AuctionId { get; set; }
    public string Vin { get; set; }
    public bool IsDeleted { get; set; }

    public abstract VehicleResponse GetVehicleResponses();

}
