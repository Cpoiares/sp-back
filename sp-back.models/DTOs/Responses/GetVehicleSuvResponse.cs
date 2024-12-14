namespace sp_back.models.DTOs.Responses;

public record GetVehicleSuvResponse : VehicleResponse
{
    public uint NumberOfSeats { get; set; }
}