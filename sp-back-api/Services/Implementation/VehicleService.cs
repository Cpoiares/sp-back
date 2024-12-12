using Microsoft.Extensions.Logging;
using sp_back_api.Database.Repository;
using sp_back_api.DTOs;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models;
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

    public async Task<Vehicle> CreateVehicleAsync(CreateVehicleRequest request)
    {
        try
        {
            ValidateVehicleDto(request);
            
            if (CheckVehicleVin(request.VIN))
                throw new ValidationException("Vin already exists in database");
            
            Vehicle vehicle = request.Type switch
            {
                VehicleType.Suv when (request.NumberOfSeats != 0 
                                      && (request.NumberOfDoors == null || request.NumberOfDoors == 0)
                                      && (request.LoadCapacity == null || request.LoadCapacity == 0)
                ) => new Suv
                {
                    VIN = request.VIN,
                    Make = request.Make,
                    Model = request.Model,
                    ProductionDate = request.ProductionDate,
                    StartingPrice = request.StartingPrice,
                    NumberOfSeats = request?.NumberOfSeats ?? throw new ValidationException("Number of seats is missing"),
                    Type = request?.Type ?? throw new ValidationException("Vehicle type is missing"),
                },
                VehicleType.Hatchback when (request.NumberOfDoors != 0 
                                          && (request.NumberOfSeats == null || request.NumberOfSeats == 0)
                                          && (request.LoadCapacity == null || request.LoadCapacity == 0)
                    ) => new Hatchback
                    {
                        VIN = request.VIN,
                        Make = request.Make,
                        Model = request.Model,
                        ProductionDate = request.ProductionDate,
                        StartingPrice = request.StartingPrice,
                        NumberOfDoors = request?.NumberOfDoors ?? throw new ValidationException("Number of doors is missing"),
                        Type = request?.Type ?? throw new ValidationException("Vehicle type is missing"),
                },
                VehicleType.Sedan when (request.NumberOfDoors != 0 
                                        && (request.NumberOfSeats == null || request.NumberOfSeats == 0)
                                        && (request.LoadCapacity == null || request.LoadCapacity == 0)
                ) => new Sedan
                {
                    VIN = request.VIN,
                    Make = request.Make,
                    Model = request.Model,
                    ProductionDate = request.ProductionDate,
                    StartingPrice = request.StartingPrice,
                    NumberOfDoors = request?.NumberOfDoors ?? throw new ValidationException("Number of doors is missing"),
                    Type = request?.Type ?? throw new ValidationException("Vehicle type is missing"),

                },
                VehicleType.Truck when (request.LoadCapacity != 0 && double.IsPositive(request?.LoadCapacity ?? throw new ValidationException("Load capacity is missing"))
                                                                  && (request.NumberOfSeats == null || request.NumberOfSeats == 0)
                                                                  && (request.NumberOfDoors == null || request.NumberOfDoors == 0)
                ) => new Truck
                {
                    VIN = request.VIN,
                    Make = request.Make,
                    Model = request.Model,
                    ProductionDate = request.ProductionDate,
                    StartingPrice = request.StartingPrice,
                    LoadCapacity = request?.LoadCapacity ?? throw new ValidationException("Load capacity is missing"),
                    Type = request?.Type ?? throw new ValidationException("Vehicle type is missing"),
                },
                _ => throw new ValidationException("Invalid vehicle type")
            };

            return await _vehicleRepository.AddAsync(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle: {Make} {Model}", request.Make, request.Model);
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

            if (request.Type.HasValue && request.Type != existingVehicle.Type)
            {
                Vehicle updatedVehicle = request.Type switch
                {
                    VehicleType.Suv when (request.NumberOfSeats is not null and not 0
                                        && request.NumberOfDoors is null or 0
                                        && request.LoadCapacity is null or 0) 
                        => new Suv
                        {
                            Id = existingVehicle.Id,
                            VIN = request.VIN ?? existingVehicle.VIN,
                            Make = request.Make ?? existingVehicle.Make,
                            Model = request.Model ?? existingVehicle.Model,
                            ProductionDate = request.ProductionDate ?? existingVehicle.ProductionDate,
                            StartingPrice = request.StartingPrice ?? existingVehicle.StartingPrice,
                            Type = VehicleType.Suv,
                            NumberOfSeats = request.NumberOfSeats.Value,
                            IsAvailable = existingVehicle.IsAvailable
                        },

                    VehicleType.Hatchback when (request.NumberOfDoors is not null and not 0
                                            && request.NumberOfSeats is null or 0
                                            && request.LoadCapacity is null or 0)
                        => new Hatchback
                        {
                            Id = existingVehicle.Id,
                            VIN = request.VIN ?? existingVehicle.VIN,
                            Make = request.Make ?? existingVehicle.Make,
                            Model = request.Model ?? existingVehicle.Model,
                            ProductionDate = request.ProductionDate ?? existingVehicle.ProductionDate,
                            StartingPrice = request.StartingPrice ?? existingVehicle.StartingPrice,
                            Type = VehicleType.Hatchback,
                            NumberOfDoors = request.NumberOfDoors.Value,
                            IsAvailable = existingVehicle.IsAvailable
                        },

                    VehicleType.Sedan when (request.NumberOfDoors is not null and not 0
                                        && request.NumberOfSeats is null or 0
                                        && request.LoadCapacity is null or 0)
                        => new Sedan
                        {
                            Id = existingVehicle.Id,
                            VIN = existingVehicle.VIN,
                            Make = request.Make ?? existingVehicle.Make,
                            Model = request.Model ?? existingVehicle.Model,
                            ProductionDate = request.ProductionDate ?? existingVehicle.ProductionDate,
                            StartingPrice = request.StartingPrice ?? existingVehicle.StartingPrice,
                            Type = VehicleType.Sedan,
                            NumberOfDoors = request.NumberOfDoors.Value,
                            IsAvailable = existingVehicle.IsAvailable
                        },

                    VehicleType.Truck when (request.LoadCapacity is not null and not 0
                                        && request.NumberOfSeats is null or 0
                                        && request.NumberOfDoors is null or 0)
                        => new Truck
                        {
                            Id = existingVehicle.Id,
                            VIN = existingVehicle.VIN,
                            Make = request.Make ?? existingVehicle.Make,
                            Model = request.Model ?? existingVehicle.Model,
                            ProductionDate = request.ProductionDate ?? existingVehicle.ProductionDate,
                            StartingPrice = request.StartingPrice ?? existingVehicle.StartingPrice,
                            Type = VehicleType.Truck,
                            LoadCapacity = request.LoadCapacity.Value,
                            IsAvailable = existingVehicle.IsAvailable
                        },

                    _ => throw new ValidationException($"Invalid vehicle type or invalid properties for type {request.Type}")
                };
                
                return await _vehicleRepository.UpdateAsync(updatedVehicle);
            }

            switch (existingVehicle.Type)
            {
                case VehicleType.Suv when request.NumberOfDoors is not null and not 0 || request.LoadCapacity is not null and not 0:
                    throw new ValidationException("Cannot add doors or load capacity to a SUV");
                    
                case VehicleType.Sedan when request.NumberOfSeats is not null and not 0 || request.LoadCapacity is not null and not 0:
                    throw new ValidationException("Cannot add seats or load capacity to a Sedan");

                case VehicleType.Hatchback when request.NumberOfSeats is not null and not 0 || request.LoadCapacity is not null and not 0:
                    throw new ValidationException("Cannot add seats or load capacity to a Hatchback");
                    
                case VehicleType.Truck when request.NumberOfDoors is not null and not 0 || request.NumberOfSeats is not null and not 0:
                    throw new ValidationException("Cannot add doors or seats to a Truck");
            }

            existingVehicle.VIN = request.VIN ?? existingVehicle.VIN;
            existingVehicle.Make = request.Make ?? existingVehicle.Make;
            existingVehicle.Model = request.Model ?? existingVehicle.Model;
            existingVehicle.ProductionDate = request.ProductionDate ?? existingVehicle.ProductionDate;
            existingVehicle.StartingPrice = request.StartingPrice ?? existingVehicle.StartingPrice;

            switch (existingVehicle)
            {
                case Suv suv when request.NumberOfSeats.HasValue:
                    suv.NumberOfSeats = request.NumberOfSeats.Value;
                    break;
                case Sedan sedan when request.NumberOfDoors.HasValue:
                    sedan.NumberOfDoors = request.NumberOfDoors.Value;
                    break;
                case Hatchback hatchback when request.NumberOfDoors.HasValue:
                    hatchback.NumberOfDoors = request.NumberOfDoors.Value;
                    break;
                case Truck truck when request.LoadCapacity.HasValue:
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
    
    public async Task<Vehicle> GetVehicleAsync(Guid id)
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
                throw new InvalidOperationException("Cannot delete vehicle that is in auction");

            await _vehicleRepository.DeleteAsync(request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle: {Id}", request.Id);
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

    private async void ValidateVehicleDto(CreateVehicleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Make))
            throw new ValidationException("Make is required");

        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ValidationException("Model is required");

        if (request.StartingPrice <= 0)
            throw new ValidationException("Starting price must be greater than 0");
    }

    private bool CheckVehicleVin(string requestVin)
    {
        return _vehicleRepository.CheckIfVinExists(requestVin);
    }
}