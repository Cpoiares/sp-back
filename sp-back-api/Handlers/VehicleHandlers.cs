using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sp_back_api.Services;
using sp_back.models.DTOs.Requests;
using sp_back.models.DTOs.Responses;

namespace sp_back_api.Handlers;

public static class VehicleHandlers
{
    public static async Task<IResult> GetVehicles(
        [AsParameters] VehicleSearchParams searchParams,
        IVehicleService vehicleService)
    {
        var vehicles = await vehicleService.SearchVehiclesAsync(searchParams);

        var response = new GetAllVehiclesResponse
        {
            Vehicles = vehicles.Select(v => v.GetVehicleResponses()).ToList()
        };
        return Results.Ok(response);
    }

    public static async Task<IResult> GetVehicleById(
        int id, 
        IVehicleService vehicleService)
    {
        var vehicle = await vehicleService.GetVehicleAsync(id);
        return Results.Ok(vehicle.GetVehicleResponses());
    }

    public static async Task<IResult> CreateSedan(
        [FromBody] CreateVehicleSedanHatchbackRequest request, 
        IVehicleService vehicleService, 
        IValidator<CreateVehicleSedanHatchbackRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var vehicle = await vehicleService.CreateSedanAsync(request);
        return Results.Created($"/vehicles/{vehicle.Id}", vehicle.GetVehicleResponses());
    }
    
    public static async Task<IResult> CreateHatchback(
        [FromBody] CreateVehicleSedanHatchbackRequest request, 
        IVehicleService vehicleService, 
        IValidator<CreateVehicleSedanHatchbackRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var vehicle = await vehicleService.CreateHatchbackAsync(request);
        return Results.Created($"/vehicles/{vehicle.Id}", vehicle.GetVehicleResponses());
    }
    
    public static async Task<IResult> CreateTruck(
        [FromBody] CreateVehicleTruckRequest request, 
        IVehicleService vehicleService, 
        IValidator<CreateVehicleTruckRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var vehicle = await vehicleService.CreateTruckAsync(request);
        return Results.Created($"/vehicles/{vehicle.Id}", vehicle.GetVehicleResponses());
    }
    
    public static async Task<IResult> CreateSuv(
        [FromBody] CreateVehicleSuvRequest request, 
        IVehicleService vehicleService, 
        IValidator<CreateVehicleSuvRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var vehicle = await vehicleService.CreateSuvAsync(request);
        return Results.Created($"/vehicles/{vehicle.Id}", vehicle.GetVehicleResponses());
    }

    public static async Task<IResult> UpdateVehicle(
        [FromBody] UpdateVehicleRequest request, 
        IValidator<UpdateVehicleRequest> validator,
        IVehicleService vehicleService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        var vehicle = await vehicleService.UpdateVehicleAsync(request);
        return Results.Ok(vehicle.GetVehicleResponses());
    }

    public static async Task<IResult> DeleteVehicle(
        [FromBody] DeleteVehicleRequest request, 
        IValidator<DeleteVehicleRequest> validator,
        IVehicleService vehicleService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        await vehicleService.DeleteVehicleAsync(request);
        return Results.NoContent();
    }

}