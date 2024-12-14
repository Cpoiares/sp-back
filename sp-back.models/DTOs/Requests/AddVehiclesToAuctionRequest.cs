using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public class AddVehiclesToAuctionRequest
{
    public int AuctionId { get; init; }
    public string[] VehicleVins { get; init; } = [];
}

public class AddVehiclesToAuctionValidator : AbstractValidator<AddVehiclesToAuctionRequest>
{
    public AddVehiclesToAuctionValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage("Auction id is required");
        
        RuleFor(x => x.VehicleVins)
            .Must(vins => vins != null && vins.Any(s => !string.IsNullOrEmpty(s)))
            .WithMessage("At least one vehicle vin is required.");
    }
}