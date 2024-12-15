using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public class RemoveVehiclesFromAuctionRequest
{
    public RemoveVehiclesFromAuctionRequest(int auctionId, string[] vehicleVins)
    {
        AuctionId = auctionId;
        VehicleVins = vehicleVins;
    }
    public string[] VehicleVins { get; set;}
    public int AuctionId { get; set;}

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