namespace sp_back_api.DTOs;

public record DeleteVehicleRequest
{
    public Guid Id { get; set; }
}