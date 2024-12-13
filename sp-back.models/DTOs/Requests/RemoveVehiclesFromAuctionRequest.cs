using FluentValidation;

namespace sp_back_api.DTOs;

public class RemoveVehiclesFromAuctionRequest
{
    public string[] VehicleVins { get; init; }
    public string AuctionName { get; init; }
    
}

public class RemoveVehiclesFromAuctionValidator : AbstractValidator<RemoveVehiclesFromAuctionRequest>
{
    public RemoveVehiclesFromAuctionValidator()
    {
        RuleFor(x => x.AuctionName)
            .NotEmpty()
            .WithMessage("Auction name is required");
        
        RuleFor(x => x.VehicleVins)
            .Must(vins => vins != null && vins.Any(s => !string.IsNullOrEmpty(s)))
            .WithMessage("At least one vehicle vin is required.");
    }
}