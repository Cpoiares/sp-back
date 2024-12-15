using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record PlaceBidInCollectiveAuctionRequest
{
    public PlaceBidInCollectiveAuctionRequest(string bidderId, int auctionId, double amount)
    {
        BidderId = bidderId;
        AuctionId = auctionId;
        Amount = amount;
    }
    public string BidderId { get; set; }
    public int AuctionId { get; set; }
    public double Amount { get; set; }
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