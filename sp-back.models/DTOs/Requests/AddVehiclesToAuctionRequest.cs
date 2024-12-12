namespace sp_back_api.DTOs;

public class AddVehiclesToAuctionRequest
{
    public string AuctionName { get; init; }
    public string[] VehicleVins { get; init; }
}