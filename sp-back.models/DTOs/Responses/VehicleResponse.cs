using sp_back.models.Enums;

namespace sp_back.models.DTOs.Responses;

public record VehicleResponse
{
    public int Id { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime ProductionDate { get; set; }
    public double StartingPrice { get; set; }
    public bool Available { get; set; }
    public bool Sold { get; set; }
    public int? AuctionId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Vin { get; set; } = string.Empty;
    public string? BidderId { get; set; }
}
