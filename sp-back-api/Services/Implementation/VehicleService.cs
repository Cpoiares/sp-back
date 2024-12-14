using Microsoft.Extensions.Logging;
using sp_back_api.Database.Repository;
using sp_back.models.DTOs.Requests;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Services.Implementation;

public class VehicleService : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        ILogger<VehicleService> logger)
    {
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }
    public async Task<Vehicle> CreateSedanAsync(CreateVehicleSedanHatchbackRequest request)
    {
        if (CheckVehicleVin(request.Vin))
            throw new ValidationException("Vin already exists in database");
        Sedan vehicle = new Sedan(request.Make, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.NumberOfDoors);
        return await _vehicleRepository.AddAsync(vehicle);
    }
    
    public async Task<Vehicle> CreateHatchbackAsync(CreateVehicleSedanHatchbackRequest request)
    {
        if (CheckVehicleVin(request.Vin))
            throw new ValidationException("Vin already exists in database");
        Hatchback vehicle = new Hatchback(request.Make, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.NumberOfDoors);
        return await _vehicleRepository.AddAsync(vehicle);
    }

    public async Task<Vehicle> CreateTruckAsync(CreateVehicleTruckRequest request)
    {
        if (CheckVehicleVin(request.Vin))
            throw new ValidationException("Vin already exists in database");
        Truck vehicle = new Truck(request.Make, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.LoadCapacity);
        return await _vehicleRepository.AddAsync(vehicle);
    }
    
    public async Task<Vehicle> CreateSuvAsync(CreateVehicleSuvRequest request)
    {
        if (CheckVehicleVin(request.Vin))
            throw new ValidationException("Vin already exists in database");
        Suv vehicle = new Suv(request.Make, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.NumberOfSeats);
        return await _vehicleRepository.AddAsync(vehicle);
    }
    
    public async Task<Vehicle> GetVehicleAsync(int id)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
                throw new NotFoundException($"Vehicle with ID {id} not found");

            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle: {Id}", id);
            throw;
        }
    }
    
    public async Task<Vehicle> GetVehicleByVin(string vin)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByVinAsync(vin);
            if (vehicle == null)
                throw new NotFoundException($"Vehicle with vin {vin} not found");

            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle: {Id}", vin);
            throw;
        }
    }

    public async Task<IEnumerable<Vehicle>> SearchVehiclesAsync(VehicleSearchParams searchParams)
    {
        try
        {
            return await _vehicleRepository.SearchAsync(searchParams);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching vehicles with parameters: {@SearchParams}", searchParams);
            throw;
        }
    }

    public async Task DeleteVehicleAsync(DeleteVehicleRequest request)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.Id)
                ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");

            if (!vehicle.IsAvailable)
                throw new InvalidOperationException("Vehicle is not available");

            await _vehicleRepository.DeleteAsync(request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle: {Id}", request.Id);
            throw;
        }
    }

public async Task<Vehicle> UpdateVehicleAsync(UpdateVehicleRequest request)
{
    try
    {
        var existingVehicle = await _vehicleRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");

        if (!existingVehicle.IsAvailable)
            throw new InvalidOperationException("Cannot update vehicle that is in auction");

        // If changing vehicle type
        if (request.Type.HasValue && request.Type != existingVehicle.Type)
        {
            // Create new vehicle of correct type using constructors
            Vehicle updatedVehicle = request.Type switch
            {
                VehicleType.Suv when request.NumberOfSeats.HasValue => 
                    new Suv(
                        request.Make ?? existingVehicle.Make,
                        request.Model ?? existingVehicle.Model,
                        request.ProductionDate ?? existingVehicle.ProductionDate,
                        request.StartingPrice ?? existingVehicle.StartingPrice,
                        existingVehicle.Vin,
                        request.NumberOfSeats.Value),

                VehicleType.Sedan when request.NumberOfDoors.HasValue => 
                    new Sedan(
                        request.Make ?? existingVehicle.Make,
                        request.Model ?? existingVehicle.Model,
                        request.ProductionDate ?? existingVehicle.ProductionDate,
                        request.StartingPrice ?? existingVehicle.StartingPrice,
                        existingVehicle.Vin,
                        request.NumberOfDoors.Value),

                VehicleType.Hatchback when request.NumberOfDoors.HasValue => 
                    new Hatchback(
                        request.Make ?? existingVehicle.Make,
                        request.Model ?? existingVehicle.Model,
                        request.ProductionDate ?? existingVehicle.ProductionDate,
                        request.StartingPrice ?? existingVehicle.StartingPrice,
                        existingVehicle.Vin,
                        request.NumberOfDoors.Value),

                VehicleType.Truck when request.LoadCapacity.HasValue => 
                    new Truck(
                        request.Make ?? existingVehicle.Make,
                        request.Model ?? existingVehicle.Model,
                        request.ProductionDate ?? existingVehicle.ProductionDate,
                        request.StartingPrice ?? existingVehicle.StartingPrice,
                        existingVehicle.Vin,
                        request.LoadCapacity.Value),

                _ => throw new ValidationException($"Invalid vehicle type or missing required properties for type {request.Type}")
            };

            // Maintain the same ID and availability
            updatedVehicle.Id = existingVehicle.Id;
            updatedVehicle.IsAvailable = existingVehicle.IsAvailable;

            return await _vehicleRepository.UpdateAsync(updatedVehicle);
        }

        // If not changing type, update only allowed properties for current type
        switch (existingVehicle)
        {
            case Suv when request.NumberOfDoors.HasValue || request.LoadCapacity.HasValue:
                throw new ValidationException("Cannot add doors or load capacity to SUV");
            
            case Sedan when request.NumberOfSeats.HasValue || request.LoadCapacity.HasValue:
                throw new ValidationException("Cannot add seats or load capacity to Sedan");
            
            case Hatchback when request.NumberOfSeats.HasValue || request.LoadCapacity.HasValue:
                throw new ValidationException("Cannot add seats or load capacity to Hatchback");
            
            case Truck when request.NumberOfDoors.HasValue || request.NumberOfSeats.HasValue:
                throw new ValidationException("Cannot add doors or seats to Truck");
        }

        // Update common properties
        if (request.Make != null) existingVehicle.Make = request.Make;
        if (request.Model != null) existingVehicle.Model = request.Model;
        if (request.ProductionDate.HasValue) existingVehicle.ProductionDate = request.ProductionDate.Value;
        if (request.StartingPrice.HasValue) existingVehicle.StartingPrice = request.StartingPrice.Value;

        // Update type-specific properties
        switch (existingVehicle)
        {
            case Suv suv:
                if (request.NumberOfSeats.HasValue)
                    suv.NumberOfSeats = request.NumberOfSeats.Value;
                break;
            case Sedan sedan:
                if (request.NumberOfDoors.HasValue)
                    sedan.NumberOfDoors = request.NumberOfDoors.Value;
                break;
            case Hatchback hatchback:
                if (request.NumberOfDoors.HasValue)
                    hatchback.NumberOfDoors = request.NumberOfDoors.Value;
                break;
            case Truck truck:
                if (request.LoadCapacity.HasValue)
                    truck.LoadCapacity = request.LoadCapacity.Value;
                break;
        }

        return await _vehicleRepository.UpdateAsync(existingVehicle);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating vehicle: {Id}", request.Id);
        throw;
    }
}

    public async Task LockVehicleInAuction(List<Vehicle> vehicles)
    {
        foreach (var v in vehicles)
        {
            v.IsAvailable = false;
            await _vehicleRepository.UpdateAsync(v);
        }
    }
    
    private bool CheckVehicleVin(string requestVin)
    {
        return _vehicleRepository.CheckIfVinExists(requestVin);
    }
}