using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sp_back.models.DTOs.Requests;
using sp_back.models.Exceptions;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Database.Repository.Implementation;

public class VehicleRepository : IVehicleRepository
{
    private readonly AuctionDbContext _context;
    private readonly ILogger<VehicleRepository> _logger;

    public VehicleRepository(AuctionDbContext context, ILogger<VehicleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle with ID: {Id}", id);
            throw;
        }
    }

    public async Task<Vehicle?> GetByVinAsync(string vin)
    {
        try
        {
            return await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Vin == vin && !v.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle with VIN: {vin}", vin);
            throw;
        }    
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        try
        {
            return await _context.Vehicles.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all vehicles");
            throw;
        }
    }

    public async Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchParams searchParams)
    {
        try
        {
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchParams.Manufacturer))
                query = query.Where(v => v.Manufacturer.Contains(searchParams.Manufacturer));
            
            if (!string.IsNullOrWhiteSpace(searchParams.Model))
                query = query.Where(v => v.Model.Contains(searchParams.Model));
            
            if (searchParams.Type.HasValue)
                query = query.Where(v => v.Type == searchParams.Type.Value);
            
            if (searchParams.YearFrom.HasValue)
                query = query.Where(v => v.ProductionDate.Year >= searchParams.YearFrom.Value);
            
            if (searchParams.YearTo.HasValue)
                query = query.Where(v => v.ProductionDate.Year <= searchParams.YearTo.Value);
            
            if (searchParams.HideNonAvailable.HasValue && searchParams.HideNonAvailable.Value)
                query = query.Where(v => v.IsAvailable);
            
            if (searchParams.HideSold.HasValue && searchParams.HideSold.Value)
                query = query.Where(v => string.IsNullOrEmpty(v.BuyerId));
            
            if (!searchParams.ShowDeleted.HasValue || searchParams.ShowDeleted.Value == false)
                query = query.Where(v => !v.IsDeleted);
            
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vehicles with parameters: {@SearchParams}", searchParams);
            throw;
        }
    }

    public async Task<Vehicle> AddAsync(Vehicle vehicle)
    {
        try
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle: {@Vehicle}", vehicle);
            throw;
        }
    }

    public async Task<Vehicle> UpdateAsync(Vehicle vehicle)
    {
        try
        {
            var existing = await _context.Vehicles.FindAsync(vehicle.Id);
            if (existing == null)
                throw new NotFoundException($"Vehicle with ID {vehicle.Id} not found");

            _context.Vehicles.Remove(existing);
            await _context.SaveChangesAsync();  
            
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle: {@Vehicle}", vehicle);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
            if (vehicle != null)
            {
                vehicle.IsDeleted = true;
                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle with ID: {Id}", id);
            throw;
        }
    }

    public bool CheckIfVinExists(string requestVin)
    {
        return _context.Vehicles.Any(v => v.Vin == requestVin && !v.IsDeleted);
    }

    public async Task MarkVehicleAsSold(int vehicleId, string buyerId)
    {
        try
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && !v.IsDeleted);
            if(vehicle == null) throw new NotFoundException($"Vehicle with ID {vehicleId} not found");
            vehicle.BuyerId = buyerId;
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking vehicle as sold for ID: {Id}", vehicleId);
            throw;
        }
        
    }
}