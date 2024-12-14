using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record CreateAuctionRequest
{
    public DateTime? EndDate { get; set; }
    public string[] VehicleVins { get; init; } = {};
}

public class CreateAuctionValidator : AbstractValidator<CreateAuctionRequest>
{
    public CreateAuctionValidator()
    {
        RuleFor(x => x.VehicleVins)
            .NotNull()
            .WithMessage("Vehicle VINs are required")
            .Must(vins => vins != null && vins.Length > 0)
            .WithMessage("At least one vehicle VIN is required");
    }
}
