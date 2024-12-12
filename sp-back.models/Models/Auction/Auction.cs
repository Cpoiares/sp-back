using sp_back.models.Enums;
using sp_back.models.Models.Vehicles;

namespace sp_back.models.Models.Auction;

public class Auction
{
    private readonly SortedSet<Bid> _bids;
    public virtual List<Vehicle> Vehicles { get; set; } = [];
    public Auction()
    {
        _bids = new SortedSet<Bid>(Comparer<Bid>.Create((a, b) => b.Amount.CompareTo(a.Amount))); // Descending order
    }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public IReadOnlyCollection<Bid> Bids => _bids;
    
    public Bid? GetHighestBidForVehicle(Guid vehicleId)
    {
        var vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException($"Vehicle {vehicleId} is not part of this auction");

        var highestBid = _bids
            .Where(b => b.VehicleId == vehicleId)
            .MaxBy(b => b.Amount);

        if (highestBid == null)
        {
            return new Bid
            {
                Id = Guid.Empty, 
                Amount = vehicle.StartingPrice,
                VehicleId = vehicleId,
                AuctionId = Id,
                BidTime = StartTime 
            };
        }

        return highestBid;
    }

    public Bid GetBidInformation(string bidderId, string vehicleVin)
    {
        return _bids.Where(b => b.BidderId == bidderId && b.Vehicle.VIN == vehicleVin).MaxBy(b => b.Amount);
    }
    public string? GetHighestBidderForVehicle(Guid vehicleId)
    {
        var vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException($"Vehicle {vehicleId} is not part of this auction");

        var highestBid = _bids
            .Where(b => b.VehicleId == vehicleId)
            .MaxBy(b => b.Amount);

        return highestBid?.BidderId; // Returns null if no bids exist
    }
}
    
    