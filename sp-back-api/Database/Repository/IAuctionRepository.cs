using sp_back.models.Models.Auction;

namespace sp_back_api.Database.Repository;

public interface IAuctionRepository
{
    Task<Auction> GetByIdAsync(Guid id);
    Task<IEnumerable<Auction>> GetActiveAuctionsAsync();
    Task<Auction> GetActiveAuctionByVehicleIdAsync(Guid vehicleId);
    Task<IEnumerable<Auction>> GetCompletedAuctionsAsync();
    Task<IEnumerable<Auction>> GetAuctionsEndingBeforeAsync(DateTime endTime);
    Task<Auction> AddAsync(Auction auction);
    Task<Auction> UpdateAsync(Auction auction);
    Task RemoveBidsForAuctionAsync(Guid auctionId);
    Task<Auction> GetAuctionByNameAsync(string auctionName);
    Task<Bid> AddBidAsync(Bid bid);
    Task<IEnumerable<Auction>> GetAllAuctionsAsync();
    Task<Auction> StartAuction(string auctionName);
    Task CloseAuctionAsync(Guid auctionId);
}