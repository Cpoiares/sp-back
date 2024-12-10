namespace sp_back_api.DTOs;

public class RemoveVehiclesFromAuctionRequest
{
    public string[] VehicleVins { get; init; }
    public string AuctionName { get; init; }
    
}