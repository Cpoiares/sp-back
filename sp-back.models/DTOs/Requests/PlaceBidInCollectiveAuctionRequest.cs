using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public class PlaceBidInCollectiveAuctionRequest
{
    public required string BidderId { get; init; }
    public required int AuctionId { get; init; }
    public required double Amount { get; init; }
}

public class PlaceBidInCollectiveAuctionValidator : AbstractValidator<PlaceBidInCollectiveAuctionRequest>
{
    public PlaceBidInCollectiveAuctionValidator()
    {
        RuleFor(x => x.BidderId)
            .NotEmpty()
            .WithMessage("Bidder Id is required");
        
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage("Auction Id is required");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0.0)
            .WithMessage("Amount has to be greater than 0.0");

    }
}