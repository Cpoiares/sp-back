using sp_back.models.Enums;
using sp_back.models.Models.Vehicles;

namespace sp_back.models.Models.Auction;

public class Auction
{
    private List<Bid> _bids;
    private List<Vehicle> _vehicles;
    private int _id;
    private DateTime? _startTime;
    private DateTime? _endTime;
    private AuctionStatus _status;
    private bool _isCollectiveAuction;

    public List<Bid> Bids
    {
        get => _bids;
        set => _bids = value;
    }

    public List<Vehicle> Vehicles
    {
        get => _vehicles;
        set => _vehicles = value;
    }

    public int Id
    {
        get => _id;
        set => _id = value;
    }

    public DateTime? StartTime
    {
        get => _startTime;
        set => _startTime = value;
    }

    public DateTime? EndTime
    {
        get => _endTime;
        set => _endTime = value;
    }

    public AuctionStatus Status
    {
        get => _status;
        set => _status = value;
    }

    public bool IsCollectiveAuction
    {
        get => _isCollectiveAuction;
        set => _isCollectiveAuction = value;
    }

    public Auction()
    {
        _status = AuctionStatus.Waiting;
        _vehicles = [];
        _bids = [];
    }
    
    public Auction(DateTime? endTime, bool isCollectiveAuction = false)
        : this()
    {
        _endTime = endTime;
        _isCollectiveAuction = isCollectiveAuction;
    }
    
    public Bid GetHighestBidForVehicle(int vehicleId)
    {
        var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException($"Vehicle {vehicleId} is not part of this auction");

        var highestBid = _bids
            .Where(b => b.VehicleId == vehicleId)
            .MaxBy(b => b.Amount);

        return highestBid ?? new Bid("", vehicle, this, vehicle.StartingPrice);
    }
    
    public Bid? GetHighestBinInHistoryForVehicle(int vehicleId)
    {
        var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException($"Vehicle {vehicleId} is not part of this auction");

        var highestBid = _bids
            .Where(b => b.VehicleId == vehicleId)
            .MaxBy(b => b.Amount);

        return highestBid;
    }

    public Bid? GetBidInformation(string bidderId, string vehicleVin)
    {
        return _bids.Where(b => b.BidderId == bidderId && b.Vehicle?.Vin == vehicleVin).MaxBy(b => b.Amount);
    }

    public string? GetHighestBidderForVehicle(int vehicleId)
    {
        var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException($"Vehicle {vehicleId} is not part of this auction");

        var highestBid = _bids
            .Where(b => b.VehicleId == vehicleId)
            .MaxBy(b => b.Amount);

        return highestBid?.BidderId; 
    }
    
    public Bid PlaceBid(string bidderId, double amount, int vehicleId)
    {
        var vehicle = _vehicles.First(v => v.Id == vehicleId);
        var bid = new Bid(bidderId, vehicle, this, amount);
        _bids.Add(bid);

        return bid;
    }
    
    public IEnumerable<Bid> PlaceCollectiveBid(string bidderId, double amount)
    {
        var bids = new List<Bid>();
        foreach (var vehicle in _vehicles)
        {
            var bid = PlaceBid(bidderId, amount, vehicle.Id);
            bids.Add(bid);
        }

        return bids;
    }
    
    public IEnumerable<Vehicle> AddVehicle(Vehicle vehicle)
    {
        Vehicles.Add(vehicle);
        return Vehicles;
    }
    
    
    public IEnumerable<Vehicle> RemoveVehicle(Vehicle vehicle)
    {

        Vehicles.Remove(vehicle);

        return Vehicles;
    }
}
    
    