using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sp_back_api.DTOs;
using sp_back_api.Services;
using sp_back.models.Models;

namespace sp_back_api.Handlers;

public static class VehicleHandlers
{
    public static async Task<IResult> GetVehicles(
        [AsParameters] VehicleSearchParams searchParams,
        IVehicleService vehicleService)
    {
        var vehicles = await vehicleService.SearchVehiclesAsync(searchParams);
        return Results.Ok(vehicles);
    }

    public static async Task<IResult> GetVehicleById(
        [FromQuery] Guid id, 
        IVehicleService vehicleService)
    {
        var vehicle = await vehicleService.GetVehicleAsync(id);
        return vehicle is null ? Results.NotFound() : Results.Ok(vehicle);
    }

    public static async Task<IResult> CreateVehicle(
        [FromBody] CreateVehicleRequest request, 
        IVehicleService vehicleService, 
        IValidator<CreateVehicleRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var vehicle = await vehicleService.CreateVehicleAsync(request);
        return Results.Created($"/vehicles/{vehicle.Id}", vehicle);
    }

    public static async Task<IResult> UpdateVehicle(
        [FromBody] UpdateVehicleRequest request, 
        IVehicleService vehicleService)
    {
        var vehicle = await vehicleService.UpdateVehicleAsync(request);
        return Results.Ok(vehicle);
    }

    public static async Task<IResult> DeleteVehicle(
        [FromBody] DeleteVehicleRequest request, 
        IVehicleService vehicleService)
    {
        await vehicleService.DeleteVehicleAsync(request);
        return Results.NoContent();
    }
}