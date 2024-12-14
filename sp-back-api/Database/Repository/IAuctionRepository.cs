using sp_back.models.Models.Auction;

namespace sp_back_api.Database.Repository;

public interface IAuctionRepository
{
    Task<Auction?> GetByIdAsync(int id);
    Task<IEnumerable<Auction>> GetActiveAuctionsAsync();
    Task<Auction?> GetActiveAuctionByVehicleIdAsync(int vehicleId);
    Task<IEnumerable<Auction>> GetCompletedAuctionsAsync();
    Task<IEnumerable<Auction>> GetAuctionsEndingBeforeAsync(DateTime endTime);
    Task<Auction> AddAsync(Auction auction);
    Task<Auction> UpdateAsync(Auction auction);
    Task RemoveBidsForAuctionAsync(int auctionId);
    Task<Bid> AddBidAsync(Bid bid);
    Task<IEnumerable<Auction>> GetAllAuctionsAsync();
    Task<Auction> StartAuction(int id);
    Task CloseAuctionAsync(int auctionId);
    Task<Auction> GetAuctionByIdAsync(int requestAuctionId);
}