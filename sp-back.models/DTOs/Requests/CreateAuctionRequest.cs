using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record CreateAuctionRequest
{
    public CreateAuctionRequest(DateTime? endDate, string[] vehicleVins)
    {
        EndDate = endDate;
        VehicleVins = vehicleVins;
    }

    public DateTime? EndDate { get; set; }
    public string[] VehicleVins { get; set;}
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
