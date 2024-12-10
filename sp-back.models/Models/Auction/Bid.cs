using sp_back.models.Models.Vehicles;

namespace sp_back.models.Models.Auction;

public class Bid : IComparable<Bid>
{
    public Guid Id { get; set; }
    public string BidderId { get; set; }
    public double Amount { get; set; }
    public DateTime BidTime { get; set; }
    public Guid AuctionId { get; set; }
    public Vehicle Vehicle { get; set; }
    public Guid VehicleId { get; set; }
    public Auction Auction { get; set; }

    public int CompareTo(Bid? other)
    {
        if (other == null) return 1;
        return Amount.CompareTo(other.Amount);
    }
}