using FluentValidation;

namespace sp_back_api.DTOs;

public class CloseAuctionRequest
{
    public string AuctionName { get; set; }
}

public class CloseAuctionValidator : AbstractValidator<CloseAuctionRequest>
{
    public CloseAuctionValidator()
    {
        RuleFor(x => x.AuctionName)
            .NotEmpty()
            .WithMessage("Auction name is required");
    }
}