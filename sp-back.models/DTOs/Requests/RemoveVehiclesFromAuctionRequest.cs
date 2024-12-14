using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public class RemoveVehiclesFromAuctionRequest
{
    public string[] VehicleVins { get; init; } = [];
    public int AuctionId { get; init; }

}

public class RemoveVehiclesFromAuctionValidator : AbstractValidator<RemoveVehiclesFromAuctionRequest>
{
    public RemoveVehiclesFromAuctionValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .NotEqual(0)
            .WithMessage("Auction id is required");
        
        RuleFor(x => x.VehicleVins)
            .Must(vins => vins != null && vins.Any(s => !string.IsNullOrEmpty(s)))
            .WithMessage("At least one vehicle vin is required.");
    }
}