using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public class CreateCollectiveAuctionRequest
{
    public DateTime? EndDate { get; init; }
    public string[] VehicleVins { get; init; } = [];
    public double StartingBid { get; init; }
}

public class CreateCollectiveAuctionRequestValidator : AbstractValidator<CreateCollectiveAuctionRequest>
{
    public CreateCollectiveAuctionRequestValidator()
    {
        
        RuleFor(x => x.VehicleVins)
            .NotNull()
            .WithMessage("Vehicle VINs are required")
            .Must(vins => vins != null && vins.Length > 0)
            .WithMessage("At least one vehicle VIN is required");
        
        RuleFor(x => x.StartingBid)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("Starting Price for this auction has to be greater than 0");
    }
}