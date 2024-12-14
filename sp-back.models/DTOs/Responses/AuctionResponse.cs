using sp_back.models.Enums;

namespace sp_back.models.DTOs.Responses;

public record AuctionResponse
{
    public int AuctionId { get; set; }
    public List<AuctionVehicles> Vehicles { get; set; } = [];
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public bool IsCollectiveAuction { get; set; }
}

public record AuctionVehicles
{
    public string Name { get; set; } = "";
    public string Vin { get; set; } = "";
    public double? WinningBid { get; set; }
    public string? BidderId { get; set; }
    public double VehicleStartingPrice { get; set; }
}