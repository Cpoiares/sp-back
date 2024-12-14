using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public class CloseAuctionRequest
{
    public int AuctionId { get; set; }
}

public class CloseAuctionValidator : AbstractValidator<CloseAuctionRequest>
{
    public CloseAuctionValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Auction id is required");
    }
}