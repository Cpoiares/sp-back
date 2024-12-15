using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record AddVehiclesToAuctionRequest
{
    public AddVehiclesToAuctionRequest(int auctionId, string[] vehicleVins)
    {
        AuctionId = auctionId;
        VehicleVins = vehicleVins;
    }

    public int AuctionId { get; set; }
    public string[] VehicleVins { get; set; }
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