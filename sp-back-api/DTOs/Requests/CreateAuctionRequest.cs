using FluentValidation;

namespace sp_back_api.DTOs;

public record CreateAuctionRequest
{
    public string Name { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
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

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Start time is required")
            .Must(startTime => startTime > DateTime.UtcNow)
            .WithMessage("Start time must be in the future")
            .When(x => x.StartTime != default);

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("End time is required")
            .Must((request, endTime) => endTime > request.StartTime)
            .WithMessage("End time must be after start time")
            .Must(endTime => endTime > DateTime.UtcNow)
            .WithMessage("End time must be in the future")
            .When(x => x.EndTime != default);

        RuleFor(x => x.VehicleVins)
            .NotNull()
            .WithMessage("Vehicle VINs are required")
            .Must(vins => vins != null && vins.Length > 0)
            .WithMessage("At least one vehicle VIN is required");
    }
}
