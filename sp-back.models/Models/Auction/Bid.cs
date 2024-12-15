using sp_back.models.Models.Vehicles;

namespace sp_back.models.Models.Auction;

public class Bid : IComparable<Bid>
{
    private int _id { get; set; }
    private string _bidderId { get; init; } = string.Empty;
    private double _amount { get; set; }
    private DateTime _bidTime { get; set; }
    private int _auctionId { get; set; }
    private Vehicle? _vehicle { get; set; }
    private int _vehicleId { get; set; }
    private Auction? _auction { get; set; }
    
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
    
    public int Id 
    {
        get => _id;
        set => _id = value;
    }
    
    public string BidderId 
    {
        get => _bidderId;
        init => _bidderId = value;
    }
    
    public double Amount 
    {
        get => _amount;
        set => _amount = value;
    }
    
    public DateTime BidTime 
    {
        get => _bidTime;
        set => _bidTime = value;
    }
    public int AuctionId 
    {
        get => _auctionId;
        set => _auctionId = value;
    }
    public Vehicle? Vehicle 
    {
        get => _vehicle;
        set => _vehicle = value;
    }
    public int VehicleId 
    {
        get => _vehicleId;
        set => _vehicleId = value;
    }
    public Auction? Auction 
    {
        get => _auction;
        set => _auction = value;
    }


    public int CompareTo(Bid? other)
    {
        if (other == null) return 1;
        return Amount.CompareTo(other.Amount);
    }
}