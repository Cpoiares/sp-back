using sp_back.models.DTOs.Requests;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Services;

public interface IVehicleService
{
    Task<Vehicle> CreateSuvAsync(CreateVehicleSuvRequest request);
    Task<Vehicle> CreateSedanAsync(CreateVehicleSedanHatchbackRequest request);
    Task<Vehicle> CreateTruckAsync(CreateVehicleTruckRequest request);
    Task<Vehicle> CreateHatchbackAsync(CreateVehicleSedanHatchbackRequest request);
    Task<Vehicle> GetVehicleAsync(int id);
    Task<IEnumerable<Vehicle>> SearchVehiclesAsync(VehicleSearchParams searchParams);
    Task LockVehicleInAuction(List<Vehicle> vehicles);
    Task DeleteVehicleAsync(DeleteVehicleRequest request);
    Task<Vehicle> UpdateVehicleAsync(UpdateVehicleRequest request);
}