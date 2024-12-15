using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record CreateCollectiveAuctionRequest
{
    public CreateCollectiveAuctionRequest(DateTime? endDate, string[] vehicleVins, double startingBid)
    {
        EndDate = endDate;
        VehicleVins = vehicleVins;
        StartingBid = startingBid;
    }
    public DateTime? EndDate { get; set; }
    public string[] VehicleVins { get; set; }
    public double StartingBid { get; set; }
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