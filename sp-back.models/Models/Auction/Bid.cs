using System.ComponentModel.DataAnnotations.Schema;
using sp_back.models.Models.Vehicles;

namespace sp_back.models.Models.Auction;

public class Bid : IComparable<Bid>
{
    public Bid(string bidderId, Vehicle vehicle, Auction auction, double amount)
    {
        BidderId = bidderId;
        Vehicle = vehicle;
        Auction = auction;
        Amount = amount;
        BidTime = DateTime.UtcNow;
    }
    
    public Bid(string bidderId)
    {
        BidderId = bidderId;
        BidTime = DateTime.UtcNow;
    }
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string BidderId { get; init; }
    public double Amount { get; set; }
    public DateTime? BidTime { get; set; }
    public int AuctionId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public int VehicleId { get; set; }
    public Auction? Auction { get; set; }

    public int CompareTo(Bid? other)
    {
        if (other == null) return 1;
        return Amount.CompareTo(other.Amount);
    }
}