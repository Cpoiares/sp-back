using sp_back_api.DTOs;
using sp_back.models.Models;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Services;

public interface IVehicleService
{
    Task<Vehicle> CreateVehicleAsync(CreateVehicleRequest request);
    Task<Vehicle> UpdateVehicleAsync(UpdateVehicleRequest request);
    Task<Vehicle> GetVehicleAsync(Guid id);
    Task<IEnumerable<Vehicle>> SearchVehiclesAsync(VehicleSearchParams searchParams);
    Task LockVehicleInAuction(List<Vehicle> vehicles);
    Task DeleteVehicleAsync(DeleteVehicleRequest request);
}