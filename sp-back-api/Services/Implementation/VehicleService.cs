﻿using Microsoft.Extensions.Logging;
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

            Vehicle vehicle = request.Type switch
            {
                VehicleType.Suv when (request.NumberOfSeats != 0 
                                      && (request.NumberOfDoors == null || request.NumberOfDoors == 0)
                                      && (request.LoadCapacity == null || request.LoadCapacity == 0)
                ) => new SUV
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
    public async Task<Vehicle> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request)
    {
        try
        {
            var existingVehicle = await _vehicleRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Vehicle with ID {id} not found");

            if (!existingVehicle.IsAvailable)
                throw new InvalidOperationException("Cannot update vehicle that is in auction");

            // If type is changing, validate the new type's requirements
            if (request.Type.HasValue && request.Type != existingVehicle.Type)
            {
                Vehicle updatedVehicle = request.Type switch
                {
                    VehicleType.Suv when (request.NumberOfSeats.HasValue && request.NumberOfSeats != 0
                                        && (!request.NumberOfDoors.HasValue || request.NumberOfDoors == 0)
                                        && (!request.LoadCapacity.HasValue || request.LoadCapacity == 0)) 
                        => new SUV
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

                    VehicleType.Hatchback when (request.NumberOfDoors.HasValue && request.NumberOfDoors != 0
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

                    VehicleType.Sedan when (request.NumberOfDoors.HasValue && request.NumberOfDoors != 0
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

                    VehicleType.Truck when (request.LoadCapacity.HasValue && request.LoadCapacity != 0
                                        && (!request.NumberOfSeats.HasValue || request.NumberOfSeats == 0)
                                        && (!request.NumberOfDoors.HasValue || request.NumberOfDoors == 0))
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
                case VehicleType.Suv when request.NumberOfDoors.HasValue || request.LoadCapacity.HasValue:
                    throw new ValidationException("Cannot add doors or load capacity to SUV");
                    
                case VehicleType.Sedan when request.NumberOfSeats.HasValue || request.LoadCapacity.HasValue:
                case VehicleType.Hatchback when request.NumberOfSeats.HasValue || request.LoadCapacity.HasValue:
                    throw new ValidationException("Cannot add seats or load capacity to Hatchback");
                    
                case VehicleType.Truck when request.NumberOfDoors.HasValue || request.NumberOfSeats.HasValue:
                    throw new ValidationException("Cannot add doors or seats to Truck");
            }

            existingVehicle.VIN = request.VIN ?? existingVehicle.VIN;
            existingVehicle.Make = request.Make ?? existingVehicle.Make;
            existingVehicle.Model = request.Model ?? existingVehicle.Model;
            existingVehicle.ProductionDate = request.ProductionDate ?? existingVehicle.ProductionDate;
            existingVehicle.StartingPrice = request.StartingPrice ?? existingVehicle.StartingPrice;

            switch (existingVehicle.Type)
            {
                case VehicleType.Suv:
                    if (request.NumberOfSeats.HasValue)
                        existingVehicle.NumberOfSeats = request.NumberOfSeats.Value;
                    break;
                case VehicleType.Sedan:
                    if (request.NumberOfDoors.HasValue)
                        existingVehicle.NumberOfDoors = request.NumberOfDoors.Value;
                    break;
                case VehicleType.Hatchback:
                    if (request.NumberOfDoors.HasValue)
                        existingVehicle.NumberOfDoors = request.NumberOfDoors.Value;
                    break;
                case VehicleType.Truck:
                    if (request.LoadCapacity.HasValue)
                        existingVehicle.LoadCapacity = request.LoadCapacity.Value;
                    break;
            }

            return await _vehicleRepository.UpdateAsync(existingVehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle: {Id}", id);
            throw;
        }
    }
    // public async Task<Vehicle> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request)
    // {
    //     try
    //     {
    //         var vehicle = await _vehicleRepository.GetByIdAsync(id) 
    //             ?? throw new NotFoundException($"Vehicle with ID {id} not found");
    //
    //         if (!vehicle.IsAvailable)
    //             throw new InvalidOperationException("Cannot update vehicle that is in auction");
    //
    //         vehicle.VIN = request.VIN ?? vehicle.VIN;
    //         vehicle.Make = request.Make ?? vehicle.Make;
    //         vehicle.Model = request.Model ?? vehicle.Model;
    //         vehicle.ProductionDate = request.ProductionDate ?? vehicle.ProductionDate;
    //         vehicle.Type = request.Type ?? vehicle.Type;
    //         vehicle.StartingPrice = request.StartingPrice ?? vehicle.StartingPrice;
    //
    //         return await _vehicleRepository.UpdateAsync(vehicle);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error updating vehicle: {Id}", id);
    //         throw;
    //     }
    // }

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

    public async Task DeleteVehicleAsync(Guid id)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Vehicle with ID {id} not found");

            if (!vehicle.IsAvailable)
                throw new InvalidOperationException("Cannot delete vehicle that is in auction");

            await _vehicleRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle: {Id}", id);
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

    private void ValidateVehicleDto(CreateVehicleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Make))
            throw new ValidationException("Make is required");

        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ValidationException("Model is required");

        if (request.StartingPrice <= 0)
            throw new ValidationException("Starting price must be greater than 0");
    }
}