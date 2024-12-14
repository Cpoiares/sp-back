namespace sp_back.models.DTOs.Responses;

public class CloseAuctionResponse
{
    public DateTime EndDate { get; set; }
    public List<AuctionVehicles> Vehicles { get; set; } = [];
}