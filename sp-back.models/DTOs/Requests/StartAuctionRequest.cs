using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record StartAuctionRequest
{
    public StartAuctionRequest(int auctionId)
    {
        AuctionId = auctionId;
    }
        
    public int AuctionId { get; set;}
}

public class StartAuctionValidator : AbstractValidator<StartAuctionRequest>
{
    public StartAuctionValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("Auction id is required");
    }
}