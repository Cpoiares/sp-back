using sp_back.models.DTOs.Responses;
using sp_back.models.Enums;


namespace sp_back.models.Models.Vehicles;

public abstract class Vehicle
{
    private int _id;
    private Auction.Auction? _auction;
    private string _manufacturer;
    private string _model;
    private DateTime _productionDate;
    private VehicleType _type;
    private double _startingPrice;
    private bool _isAvailable;
    private string? _buyerId;
    private int? _auctionId;
    private string _vin;
    private bool _isDeleted;

    protected Vehicle(string manufacturer, string model, DateTime productionDate, double startingPrice, string vin)
    {
        _manufacturer = manufacturer;
        _model = model;
        _productionDate = productionDate;
        _startingPrice = startingPrice;
        _vin = vin;
        _isDeleted = false;
        _isAvailable = true;
    }

    public int Id
    {
        get => _id;
        set => _id = value;
    }

    public virtual Auction.Auction? Auction
    {
        get => _auction;
        init => _auction = value;
    }

    public string Manufacturer
    {
        get => _manufacturer;
        set => _manufacturer = value;
    }

    public string Model
    {
        get => _model;
        set => _model = value;
    }

    public DateTime ProductionDate
    {
        get => _productionDate;
        set => _productionDate = value;
    }

    public string Year => ProductionDate.Year.ToString();

    public VehicleType Type
    {
        get => _type;
        init => _type = value;
    }

    public double StartingPrice
    {
        get => _startingPrice;
        set => _startingPrice = value;
    }

    public bool IsAvailable
    {
        get => _isAvailable;
        set => _isAvailable = value;
    }

    public bool IsSold => !string.IsNullOrEmpty(_buyerId);

    public string? BuyerId
    {
        get => _buyerId;
        set => _buyerId = value;
    }

    public int? AuctionId
    {
        get => _auctionId;
        set => _auctionId = value;
    }

    public string Vin
    {
        get => _vin;
        set => _vin = value;
    }

    public bool IsDeleted
    {
        get => _isDeleted;
        set => _isDeleted = value;
    }

    public abstract VehicleResponse GetVehicleResponses();
}