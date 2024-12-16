namespace sp_back.models.DTOs.Responses;

public class StartAuctionResponse
{
    public DateTime StartDate { get; set; } 
    public List<AuctionVehicles> Vehicles { get; set; } = [];
    public int AuctionId { get; set; }
}