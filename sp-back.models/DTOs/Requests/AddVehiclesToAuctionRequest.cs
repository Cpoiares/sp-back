using FluentValidation;

namespace sp_back_api.DTOs;

public class AddVehiclesToAuctionRequest
{
    public string AuctionName { get; init; }
    public string[] VehicleVins { get; init; }
}

public class AddVehiclesToAuctionValidator : AbstractValidator<AddVehiclesToAuctionRequest>
{
    public AddVehiclesToAuctionValidator()
    {
        RuleFor(x => x.AuctionName)
            .NotEmpty()
            .WithMessage("Auction name is required");
        
        RuleFor(x => x.VehicleVins)
            .Must(vins => vins != null && vins.Any(s => !string.IsNullOrEmpty(s)))
            .WithMessage("At least one vehicle vin is required.");
    }
}