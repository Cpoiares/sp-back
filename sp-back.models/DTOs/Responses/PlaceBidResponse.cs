namespace sp_back.models.DTOs.Responses;

public record PlaceBidResponse
{
    public int? BidId { get; set; }
    public double Amount { get; set; }
    public DateTime? BidTime { get; set; }
    public string Bidder { get; set; } = "";
    public string Vehicle { get; set; } = "";
}