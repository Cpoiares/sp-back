namespace sp_back.models.DTOs.Responses;

public record GetVehicleTruckResponse : VehicleResponse
{
    public double LoadCapacity { get; set; }
}