namespace sp_back_api.DTOs.Responses;

public record GetAllActiveAuctionsResponse
{
    public List<AuctionResponse> ActiveAuctions { get; set; }
}