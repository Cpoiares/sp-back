namespace sp_back_api.DTOs.Responses;

public class VehicleResponse
{
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public DateTime ProductionDate { get; set; }
    public uint NumberOfSeats { get; set; }
    public uint NumberOfDoors { get; set; }
    public double LoadCapacity { get; set; }
    public double StartingPrice { get; set; }
    public bool Available { get; set; }
    public bool Sold { get; set; }
    public Guid AuctionId { get; set; }
}