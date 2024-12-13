using sp_back_api.DTOs.Responses;

namespace sp_back_api.DTOs;

public class GetAllVehiclesResponse
{
    public List<VehicleResponse> Vehicles { get; set; }
}