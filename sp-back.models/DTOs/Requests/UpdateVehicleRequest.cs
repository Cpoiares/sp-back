using sp_back.models.Enums;

namespace sp_back_api.DTOs;

public record UpdateVehicleRequest
{
    public Guid Id { get; set; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public DateTime? ProductionDate { get; init; }
    public uint? NumberOfDoors { get; init; }
    public uint? NumberOfSeats { get; init; }
    public double? LoadCapacity { get; init;}
    public VehicleType? Type { get; init; }
    public double? StartingPrice { get; init; }
    public string? VIN { get; init; }
}