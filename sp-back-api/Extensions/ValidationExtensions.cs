﻿using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using sp_back_api.DTOs;

namespace sp_back_api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateVehicleValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateAuctionValidator>();
        return services;
    }
}