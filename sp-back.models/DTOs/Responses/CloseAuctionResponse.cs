namespace sp_back_api.DTOs.Responses;

public class CloseAuctionResponse
{
    public string AuctionName { get; set; }
    public DateTime EndDate { get; set; }
    public List<AuctionVehicles> Vehicles { get; set; }
}