using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record PlaceBidRequest
{
    public PlaceBidRequest(string bidderId, string vehicleVin, double amount)
    {
        BidderId = bidderId;
        VehicleVin = vehicleVin;
        Amount = amount;
    }
    public string BidderId { get;}
    public string VehicleVin { get; }
    public double Amount { get; set; }
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