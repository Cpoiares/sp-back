namespace sp_back_api.DTOs.Responses;

public record PlaceBidResponse
{
    public Guid BidId { get; set; }
    public double Amount { get; set; }
    public DateTime BidTime { get; set; }
    public string Bidder { get; set; }
    public string Vehicle { get; set; }
}