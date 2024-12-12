namespace sp_back_api.DTOs.Responses;

public class StartAuctionResponse
{
    public string AuctionName { get; set; }
    public DateTime StartDate { get; set; }
    public List<AuctionVehicles> Vehicles { get; set; }
}