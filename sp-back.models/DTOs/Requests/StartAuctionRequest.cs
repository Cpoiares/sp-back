namespace sp_back.models.DTOs.Requests;

public record StartAuctionRequest
{
    public string AuctionName { get; set; }
}