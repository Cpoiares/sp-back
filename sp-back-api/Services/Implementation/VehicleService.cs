using Microsoft.Extensions.Logging;
using sp_back_api.Database.Repository;
using sp_back.models.DTOs.Requests;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Vehicles;
using InvalidOperationException = sp_back.models.Exceptions.InvalidOperationException;

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
        _logger.LogInformation($"Creating Sedan Vehicle: Checking Vehicle Vin: {request.Vin}");

        if (CheckVehicleVin(request.Vin))
            throw new ValidationException($"Vin {request.Vin} already exists in database");
        Sedan vehicle = new Sedan(request.Manufacturer, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.NumberOfDoors);
        var v = await _vehicleRepository.AddAsync(vehicle);
        _logger.LogInformation($"Created Sedan Vehicle: {v.Id}");
        return v;
    }
    
    public async Task<Vehicle> CreateHatchbackAsync(CreateVehicleSedanHatchbackRequest request)
    {
        _logger.LogInformation($"Creating Hatchback Vehicle: Checking Vehicle Vin: {request.Vin}");

        if (CheckVehicleVin(request.Vin))
            throw new ValidationException($"Vin {request.Vin} already exists in database");
        Hatchback vehicle = new Hatchback(request.Manufacturer, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.NumberOfDoors);
        var v = await _vehicleRepository.AddAsync(vehicle);
        _logger.LogInformation($"Created Hatchback Vehicle: {v.Id}");
        return v;
    }

    public async Task<Vehicle> CreateTruckAsync(CreateVehicleTruckRequest request)
    {
        _logger.LogInformation($"Creating Truck Vehicle: Checking Vehicle Vin: {request.Vin}");

        if (CheckVehicleVin(request.Vin))
            throw new ValidationException($"Vin {request.Vin} already exists in database");
        Truck vehicle = new Truck(request.Manufacturer, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.LoadCapacity);
        var v = await _vehicleRepository.AddAsync(vehicle);
        _logger.LogInformation($"Created Truck Vehicle: {v.Id}");
        return v;
    }
    
    public async Task<Vehicle> CreateSuvAsync(CreateVehicleSuvRequest request)
    {
        _logger.LogInformation($"Creating Suv Vehicle: Checking Vehicle Vin: {request.Vin}");

        if (CheckVehicleVin(request.Vin))
            throw new ValidationException($"Vin {request.Vin} already exists in database");
        Suv vehicle = new Suv(request.Manufacturer, request.Model, request.ProductionDate, request.StartingPrice, request.Vin, request.NumberOfSeats);
        var v = await _vehicleRepository.AddAsync(vehicle);
        _logger.LogInformation($"Created Suv Vehicle: {v.Id}");
        return v;
    }
    
    public async Task<Vehicle> GetVehicleAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Getting vehicle with id {id}");

            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
                throw new NotFoundException($"Vehicle with ID {id} not found");
            
            _logger.LogInformation($"Retrieving vehicle {vehicle}");

            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Vehicle>> SearchVehiclesAsync(VehicleSearchParams searchParams)
    {
        try
        {
            _logger.LogInformation($"Getting vehicles for search criteria {searchParams.GetAsString()}");

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
            _logger.LogInformation($"Deleting vehicle with id {request.Id}");

            var vehicle = await _vehicleRepository.GetByIdAsync(request.Id)
                ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");

            if (!vehicle.IsAvailable)
                throw new InvalidOperationException("Vehicle is not available for deletion as its part of an ongoing auction");

            await _vehicleRepository.DeleteAsync(request.Id);
            
            _logger.LogInformation($"Marked vehicle with id {request.Id} as deleted");

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
        _logger.LogInformation($"Initiating update on {request.Type} (Id: {request.Id})");
        var existingVehicle = await _vehicleRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");

        if (!existingVehicle.IsAvailable)
            throw new InvalidOperationException("Cannot update vehicle that is in auction");

        if (request.Type.HasValue && request.Type != existingVehicle.Type)
        {
            _logger.LogInformation($"Performing vehicle type change on {request.Type} (Id: {request.Id}) from {existingVehicle.Type} to {request.Type}");
            Vehicle updatedVehicle = request.Type switch
            {
                VehicleType.Suv when request.NumberOfSeats.HasValue => 
                    new Suv(
                        request.Manufacturer ?? existingVehicle.Manufacturer,
                        request.Model ?? existingVehicle.Model,
                        request.ProductionDate ?? existingVehicle.ProductionDate,
                        request.StartingPrice ?? existingVehicle.StartingPrice,
                        existingVehicle.Vin,
                        request.NumberOfSeats.Value),

                VehicleType.Sedan when request.NumberOfDoors.HasValue => 
                    new Sedan(
                        request.Manufacturer ?? existingVehicle.Manufacturer,
                        request.Model ?? existingVehicle.Model,
                        request.ProductionDate ?? existingVehicle.ProductionDate,
                        request.StartingPrice ?? existingVehicle.StartingPrice,
                        existingVehicle.Vin,
                        request.NumberOfDoors.Value),

                VehicleType.Hatchback when request.NumberOfDoors.HasValue => 
                    new Hatchback(
                        request.Manufacturer ?? existingVehicle.Manufacturer,
                        request.Model ?? existingVehicle.Model,
                        request.ProductionDate ?? existingVehicle.ProductionDate,
                        request.StartingPrice ?? existingVehicle.StartingPrice,
                        existingVehicle.Vin,
                        request.NumberOfDoors.Value),

                VehicleType.Truck when request.LoadCapacity.HasValue => 
                    new Truck(
                        request.Manufacturer ?? existingVehicle.Manufacturer,
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
            var v = await _vehicleRepository.UpdateAsync(updatedVehicle);
            _logger.LogInformation($"Updated vehicle with id {existingVehicle.Id}");
            return v;
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

        if (request.Manufacturer != null) existingVehicle.Manufacturer = request.Manufacturer;
        if (request.Model != null) existingVehicle.Model = request.Model;
        if (request.ProductionDate.HasValue) existingVehicle.ProductionDate = request.ProductionDate.Value;
        if (request.StartingPrice.HasValue) existingVehicle.StartingPrice = request.StartingPrice.Value;

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
        var update = await _vehicleRepository.UpdateAsync(existingVehicle);
        _logger.LogInformation($"Updated vehicle with id {existingVehicle.Id}");
        return update;
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
            _logger.LogInformation($"Successfully locked vehicle with id {v.Id}");
        }
    }
    
    private bool CheckVehicleVin(string requestVin)
    {
        return _vehicleRepository.CheckIfVinExists(requestVin);
    }
}