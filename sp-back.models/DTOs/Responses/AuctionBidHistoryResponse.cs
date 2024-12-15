namespace sp_back.models.DTOs.Responses;

public record AuctionBidHistoryResponse
{
    public int AuctionId { get; set; }
    public bool IsCollectiveAuction { get; set; }
    public IEnumerable<AuctionBids> Bids { get; set; } = [];
}

public record AuctionBids
{
    public int? VehicleId { get; set; }
    public double BidAmount { get; set; }
    public string BidderId { get; set; } = string.Empty;
    public DateTime BidTime { get; set; }
    public bool IsWinnerBid { get; set; }
}
