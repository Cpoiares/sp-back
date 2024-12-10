using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Auction;

namespace sp_back_api.Database.Repository.Implementation;

public class AuctionRepository : IAuctionRepository
{
    private readonly AuctionDbContext _context;
    private readonly ILogger<AuctionRepository> _logger;

    public AuctionRepository(AuctionDbContext context, ILogger<AuctionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Auction> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.Auctions
                .Include(a => a.Vehicles)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auction with ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Auction>> GetActiveAuctionsAsync()
    {
        try
        {
            return await _context.Auctions
                .Include(a => a.Vehicles)
                .Include(a => a.Bids)
                .Where(a => a.Status == AuctionStatus.Active)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active auctions");
            throw;
        }
    }

    public async Task<Auction> GetActiveAuctionByVehicleIdAsync(Guid vehicleId)
    {
        try
        {
            return await _context.Auctions
                .FirstOrDefaultAsync(a =>
                    (a.Vehicles.Any(v => v.Id == vehicleId) &&
                     (a.Status == AuctionStatus.Active || a.Status == AuctionStatus.Scheduled)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active auction for vehicle: {VehicleId}", vehicleId);
            throw;
        }
    }

    public async Task<IEnumerable<Auction>> GetCompletedAuctionsAsync()
    {
        try
        {
            return await _context.Auctions
                .Include(a => a.Vehicles)
                .Where(a => a.Status == AuctionStatus.Completed)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving completed auctions");
            throw;
        }
    }

    public async Task<IEnumerable<Auction>> GetAuctionsEndingBeforeAsync(DateTime endTime)
    {
        try
        {
            return await _context.Auctions
                .Include(a => a.Vehicles)
                .Where(a =>
                    a.Status == AuctionStatus.Active &&
                    a.EndTime <= endTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auctions ending before: {EndTime}", endTime);
            throw;
        }
    }

    public async Task<Auction> AddAsync(Auction auction)
    {
        try
        {
            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding auction: {@Auction}", auction);
            throw;
        }
    }

    public async Task<Auction> UpdateAsync(Auction auction)
    {
        try
        {
            var existingAuction = await _context.Auctions
                .Include(a => a.Vehicles)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == auction.Id);

            if (existingAuction == null)
                throw new NotFoundException($"Auction with ID {auction.Id} not found");

            // Detach existing entity
            _context.Entry(existingAuction).State = EntityState.Detached;

            // Attach and mark as modified
            _context.Auctions.Attach(auction);
            _context.Entry(auction).State = EntityState.Modified;

            // Mark new bids as added
            foreach (var bid in auction.Bids.Where(b => b.Id == Guid.Empty))
            {
                _context.Entry(bid).State = EntityState.Added;
            }

            await _context.SaveChangesAsync();
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating auction: {AuctionId}", auction.Id);
            throw;
        }
    }

    public async Task RemoveBidsForAuctionAsync(Guid auctionId)
    {
        var bidsToRemove = await _context.Bids
            .Where(b => b.AuctionId == auctionId)
            .ToListAsync();

        _context.Bids.RemoveRange(bidsToRemove);
        await _context.SaveChangesAsync();
    }


    public async Task<Bid> AddBidAsync(Bid bid)
    {
        try
        {
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();
            return bid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding bid: {@Bid}", bid);
            throw;
        }
    }

    public async Task<Auction> GetAuctionByNameAsync(string auctionName)
    {
        try
        {
            return await _context.Auctions
                .Include(a => a.Vehicles)
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Name == auctionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auction with name: {auctionName}", auctionName);
            throw;
        }    
    }
    

    public async Task<IEnumerable<Auction>> GetScheduledAuctionsAsync()
    {
        try
        {
            return await _context.Auctions
                .Include(a => a.Vehicles)
                .Where(a => a.Status == AuctionStatus.Scheduled && a.StartTime <= DateTime.UtcNow)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving scheduled auctions");
            throw;
        }
    }
    
}