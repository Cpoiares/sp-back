using sp_back.models.Enums;

namespace sp_back_api.DTOs;

public record UpdateVehicleRequest
{
    public string? Make { get; init; }
    public string? Model { get; init; }
    public DateTime? ProductionDate { get; init; }
    public int? NumberOfDoors { get; init; }
    public int? Year { get; init; }
    public VehicleType? Type { get; init; }
    public double? StartingPrice { get; init; }
    public string? VIN { get; init; }
}