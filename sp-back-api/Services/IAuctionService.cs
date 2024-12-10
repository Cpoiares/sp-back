using sp_back_api.DTOs;
using sp_back.models.Models.Auction;

namespace sp_back_api.Services;

public interface IAuctionService
{
    Task<Auction> CreateAuctionAsync(CreateAuctionRequest request);
    Task<Auction> GetAuctionAsync(Guid id);
    Task<IEnumerable<Auction>> GetActiveAuctionsAsync();
    Task<Auction> PlaceBidAsync(PlaceBidRequest request);
    Task<Auction> CancelAuctionAsync(string auctionName);
    Task<Auction> CloseAuctionAsync(string auctionName);
    Task ProcessCompletedAuctionsAsync();
    Task<Auction?> AddVehiclesToAuctionAsync(AddVehiclesToAuctionRequest request);
    Task<Auction?> RemoveVehiclesFromAuctionAsync(RemoveVehiclesFromAuctionRequest request);
}