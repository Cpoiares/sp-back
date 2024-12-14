using sp_back.models.DTOs.Requests;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Database.Repository;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(int id);
    Task<Vehicle?> GetByVinAsync(string vin);
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchParams searchParams);
    Task<Vehicle> AddAsync(Vehicle vehicle);
    Task<Vehicle> UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(int id);
    bool CheckIfVinExists(string requestVin);
    Task MarkVehicleAsSold(int id, string buyerId);
}