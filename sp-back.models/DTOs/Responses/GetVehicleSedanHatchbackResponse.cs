namespace sp_back.models.DTOs.Responses;

public record GetVehicleSedanHatchbackResponse : VehicleResponse
{
    public uint NumberOfDoors { get; set; }
}