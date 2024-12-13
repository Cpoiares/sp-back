using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using sp_back_api.DTOs;
using sp_back.models.DTOs.Requests;

namespace sp_back_api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateVehicleValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<DeleteVehicleValidator>();
        services.AddValidatorsFromAssemblyContaining<RemoveVehiclesFromAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<AddVehiclesToAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<CloseAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<StartAuctionValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateVehicleValidator>();
        services.AddValidatorsFromAssemblyContaining<PlaceBidValidator>();

        return services;
    }
}