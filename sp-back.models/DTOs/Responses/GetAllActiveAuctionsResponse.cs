namespace sp_back.models.DTOs.Responses;

public record GetAllActiveAuctionsResponse
{
    public List<AuctionResponse> Auctions { get; set; } = [];
}