using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using sp_back.models.DTOs.Requests;

namespace sp_back_api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<DeleteVehicleValidator>();
        services.AddValidatorsFromAssemblyContaining<RemoveVehiclesFromAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<AddVehiclesToAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<CloseAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<StartAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateVehicleSuvRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateVehicleTruckRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateVehicleSedanHatchbackValidator>();
        services.AddValidatorsFromAssemblyContaining<PlaceBidValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateCollectiveAuctionRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<PlaceBidInCollectiveAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateVehicleValidator>();

        return services;
    }
}