using sp_back.models.Models;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Database.Repository;

public interface IVehicleRepository
{
    Task<Vehicle> GetByIdAsync(Guid id);
    Task<Vehicle> GetByVINAsync(string vin);
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchParams searchParams);
    Task<Vehicle> AddAsync(Vehicle vehicle);
    Task<Vehicle> UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(Guid id);
    bool CheckIfVinExists(string requestVin);
}