namespace sp_back_api.DTOs.Responses;

public class CancelAuctionRequest
{
    public string AuctionName { get; set; }
    public bool Cancel { get; set; }
}