using sp_back.models.Enums;

namespace sp_back_api.DTOs.Responses;

public record VehicleResponse
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public DateTime ProductionDate { get; set; }
    public uint? NumberOfSeats { get; set; }
    public uint? NumberOfDoors { get; set; }
    public double? LoadCapacity { get; set; }
    public double StartingPrice { get; set; }
    public bool Available { get; set; }
    public bool Sold { get; set; }
    public Guid? AuctionId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Vin { get; set; }
    public string? BidderId { get; set; }
}
