using sp_back.models.Enums;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.DTOs.Responses;

public record AuctionResponse
{
    public string AuctionName { get; set; }
    public Guid AuctionId { get; set; }
    public List<AuctionVehicles> Vehicles { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; }
}

public record AuctionVehicles
{
    public string Name { get; set; }
    public string VIN { get; set; }
    public double CurrentBid { get; set; }
    public string BidderId { get; set; }
}