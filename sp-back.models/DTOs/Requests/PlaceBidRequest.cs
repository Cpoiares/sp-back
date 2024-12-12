namespace sp_back_api.DTOs;

public record PlaceBidRequest
{
    public required string BidderId { get; init; }
    public required string VehicleVin { get; init; }
    public required double Amount { get; init; }
}