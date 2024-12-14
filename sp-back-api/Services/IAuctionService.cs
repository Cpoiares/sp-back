using sp_back.models.DTOs.Requests;
using sp_back.models.Models.Auction;

namespace sp_back_api.Services;

public interface IAuctionService
{
    Task<Auction> CreateAuctionAsync(CreateAuctionRequest request);
    Task<Auction> GetAuctionAsync(int id);
    Task<IEnumerable<Auction>> GetActiveAuctionsAsync();
    Task<Auction> PlaceBidAsync(PlaceBidRequest request);
    Task<Auction> CancelAuctionAsync(int auctionId);
    Task<Auction> CloseAuctionAsync(int auctionId);
    Task ProcessCompletedAuctionsAsync();
    Task<Auction> AddVehiclesToAuctionAsync(AddVehiclesToAuctionRequest request);
    Task<Auction> RemoveVehiclesFromAuctionAsync(RemoveVehiclesFromAuctionRequest request);
    Task<IEnumerable<Auction>> GetAllAuctionsAsync();
    Task<Auction> StartAuctionAsync(int auctionId);
    Task<Auction> CreateCollectiveAuction(CreateCollectiveAuctionRequest request);
    Task<Auction> PlaceBidInCollectiveAuction(PlaceBidInCollectiveAuctionRequest request);
    Task<IEnumerable<Auction>> GetCompletedAuctionsAsync();
}