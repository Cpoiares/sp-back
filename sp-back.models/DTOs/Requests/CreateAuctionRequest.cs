using FluentValidation;

namespace sp_back_api.DTOs;

public record CreateAuctionRequest
{
    public string Name { get; init; }
    public DateTime? EndDate { get; init; }
    public string[] VehicleVins { get; init; } = {};
}

public class CreateAuctionValidator : AbstractValidator<CreateAuctionRequest>
{
    public CreateAuctionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Auction name is required")
            .MaximumLength(100)
            .WithMessage("Auction name cannot exceed 100 characters");
        
        RuleFor(x => x.VehicleVins)
            .NotNull()
            .WithMessage("Vehicle VINs are required")
            .Must(vins => vins != null && vins.Length > 0)
            .WithMessage("At least one vehicle VIN is required");
    }
}
