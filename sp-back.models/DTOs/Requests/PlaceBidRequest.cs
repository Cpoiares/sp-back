using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record PlaceBidRequest
{
    public required string BidderId { get; init; }
    public required string VehicleVin { get; init; }
    public required double Amount { get; init; }
}

public class PlaceBidValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidValidator()
    {
        RuleFor(x => x.BidderId)
            .NotEmpty()
            .WithMessage("Bidder Id is required");
        
        RuleFor(x => x.VehicleVin)
            .NotEmpty()
            .WithMessage("VehicleVin is required");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0.0)
            .WithMessage("Amount has to be greater than 0.0");

    }
}