using sp_back.models.Enums;
using sp_back.models.Models.Vehicles;

namespace sp_back.models.Models.Auction;

public class Auction
{
    public List<Bid> Bids { get; set; }
    public List<Vehicle> Vehicles { get; set; }

    public Auction()
    {
        Status = AuctionStatus.Waiting;
        Vehicles = [];
        Bids = [];
    }
    
    public Auction(DateTime? endTime, bool isCollectiveAuction = false)
        : this()
    {
        EndTime = endTime;
        IsCollectiveAuction = isCollectiveAuction;
    }
    
    public int Id { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public bool IsCollectiveAuction { get; set; }
    
    public Bid GetHighestBidForVehicle(int vehicleId)
    {
        var vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException($"Vehicle {vehicleId} is not part of this auction");

        var highestBid = Bids
            .Where(b => b.VehicleId == vehicleId)
            .MaxBy(b => b.Amount);

        return highestBid ?? new Bid("", vehicle, this, vehicle.StartingPrice);
    }

    public Bid? GetBidInformation(string bidderId, string vehicleVin)
    {
        return Bids.Where(b => b.BidderId == bidderId && b.Vehicle?.Vin == vehicleVin).MaxBy(b => b.Amount);
    }
    public string? GetHighestBidderForVehicle(int vehicleId)
    {
        var vehicle = Vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException($"Vehicle {vehicleId} is not part of this auction");

        var highestBid = Bids
            .Where(b => b.VehicleId == vehicleId)
            .MaxBy(b => b.Amount);

        return highestBid?.BidderId; 
    }
    
    public Bid PlaceBid(string bidderId, double amount, int vehicleId)
    {

        var vehicle = Vehicles.First(v => v.Id == vehicleId);
        var bid = new Bid(bidderId, vehicle, this, amount);
        Bids.Add(bid);

        return bid;
    }
    
    public IEnumerable<Bid> PlaceCollectiveBid(string bidderId, double amount)
    {
        var bids = new List<Bid>();
        foreach (var vehicle in Vehicles)
        {
            var bid = PlaceBid(bidderId, amount, vehicle.Id);
            bids.Add(bid);
        }

        return bids;
    }
    
    public IEnumerable<Vehicle> AddVehicles(List<Vehicle> vehicles)
    {
        Vehicles.AddRange(vehicles);

        return Vehicles;
    }
    
    public IEnumerable<Vehicle> RemoveVehicles(List<Vehicle> vehicles)
    {
        foreach (var vehicle in vehicles)
        {
            Vehicles.Remove(vehicle);
        }

        return Vehicles;
    }
    
}
    
    