using sp_back.models.Models.Auction;
namespace sp_back_api.Loging;

public interface IAuctionLogger
{
    Task LogAuctionCompleted(Auction auction);
    
}