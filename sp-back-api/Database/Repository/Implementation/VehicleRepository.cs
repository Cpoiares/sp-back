using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sp_back.models.Exceptions;
using sp_back.models.Models;
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

    public async Task<Vehicle> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id) ?? throw new NotFoundException("No vehicles found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle with ID: {Id}", id);
            throw;
        }
    }

    public async Task<Vehicle> GetByVINAsync(string vin)
    {
        try
        {
            return await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VIN == vin) ?? throw new NotFoundException("No vehicles found");
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

            if (!string.IsNullOrWhiteSpace(searchParams.Make))
                query = query.Where(v => v.Make.Contains(searchParams.Make));
            
            if (!string.IsNullOrWhiteSpace(searchParams.Model))
                query = query.Where(v => v.Model.Contains(searchParams.Model));
            
            if (searchParams.Type.HasValue)
                query = query.Where(v => v.Type == searchParams.Type.Value);
            
            if (searchParams.IsAvailable.HasValue)
                query = query.Where(v => v.IsAvailable == searchParams.IsAvailable.Value);
            
            if (searchParams.NumberOfDoors.HasValue)
                query = query.Where(v => v.NumberOfDoors >= searchParams.NumberOfDoors.Value);
            
            if (searchParams.NumberOfSeats.HasValue)
                query = query.Where(v => v.NumberOfSeats >= searchParams.NumberOfSeats.Value);
            
            if (searchParams.LoadCapacity.HasValue)
                query = query.Where(v => v.LoadCapacity >= searchParams.LoadCapacity.Value);
            
            // Apply pagination if specified
            if (searchParams.Page.HasValue && searchParams.PageSize.HasValue)
            {
                int skip = (searchParams.Page.Value - 1) * searchParams.PageSize.Value;
                query = query.Skip(skip).Take(searchParams.PageSize.Value);
            }

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
            _context.ChangeTracker.Clear();
            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle: {@Vehicle}", vehicle);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var vehicle = await GetByIdAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle with ID: {Id}", id);
            throw;
        }
    }
}