using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record StartAuctionRequest
{
    public string AuctionName { get; set; }
}

public class StartAuctionValidator : AbstractValidator<StartAuctionRequest>
{
    public StartAuctionValidator()
    {
        RuleFor(x => x.AuctionName)
            .NotEmpty()
            .WithMessage("Auction name is required");
    }
}