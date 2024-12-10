using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sp_back_api.Database.Repository;
using sp_back_api.DTOs;
using sp_back_api.Loging;
using sp_back.models.Config;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Auction;
using sp_back.models.Models.Vehicles;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace sp_back_api.Services.Implementation;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleService _vehicleService;
    private readonly IAuctionLogger _auctionLogger;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AuctionService> _logger;

    public AuctionService(
        IAuctionRepository auctionRepository,
        IVehicleRepository vehicleRepository,
        IAuctionLogger auctionLogger,
        IOptions<AppSettings> appSettings,
        ILogger<AuctionService> logger, 
        IVehicleService vehicleService)
    {
        _auctionRepository = auctionRepository;
        _vehicleRepository = vehicleRepository;
        _auctionLogger = auctionLogger;
        _appSettings = appSettings.Value;
        _logger = logger;
        _vehicleService = vehicleService;
    }

    public async Task<Auction> CreateAuctionAsync(CreateAuctionRequest request)
    {
        try
        {
            var auctionVehicles = new List<Vehicle>();
            ValidateAuctionDto(request);
            foreach (var vin in request.VehicleVins)
            {
                var vehicle = await _vehicleRepository.GetByVINAsync(vin)
                              ?? throw new NotFoundException($"Vehicle with ID {vin} not found");
                
                var a = await _auctionRepository.GetActiveAuctionByVehicleIdAsync(vehicle.Id);
                if (a != null)
                    throw new InvalidOperationException("Vehicle is already in an active auction");
                if (!vehicle.IsAvailable)
                    throw new InvalidOperationException("Vehicle is not available for auction");
                if (vehicle.IsSold)
                    throw new InvalidOperationException("Vehicle is not available for auction as it was already sold");

                vehicle.IsAvailable = false;
                auctionVehicles.Add(vehicle);
            }
            
            var auction = new Auction
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                StartTime = request.StartTime.ToUniversalTime(),
                Vehicles = auctionVehicles,
                EndTime = request.EndTime.ToUniversalTime(),
                Status = request.StartTime <= DateTime.UtcNow ? 
                    AuctionStatus.Active : AuctionStatus.Scheduled
            };

            await _vehicleService.LockVehicleInAuction(auctionVehicles);
            return await _auctionRepository.AddAsync(auction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auction with vehicles: {VehicleId}", string.Join(',', request.VehicleVins));
            throw;
        }
    }

    public async Task<Auction> GetAuctionAsync(Guid id)
    {
        try
        {
            var auction = await _auctionRepository.GetByIdAsync(id);
            if (auction == null)
                throw new NotFoundException($"Auction with ID {id} not found");

            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auction: {Id}", id);
            throw;
        }
    }
    public async Task<Auction> GetAuctionByNameAsync(string auctionName)
    {
        try
        {
            var auction = await _auctionRepository.GetAuctionByNameAsync(auctionName);
            if (auction == null)
                throw new NotFoundException($"Auction with name {auctionName} not found");

            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auction: {auctionName}", auctionName);
            throw;
        }
    }

    public async Task<IEnumerable<Auction>> GetActiveAuctionsAsync()
    {
        try
        {
            return await _auctionRepository.GetActiveAuctionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active auctions");
            throw;
        }
    }

    public async Task<Auction> PlaceBidAsync(PlaceBidRequest request)
    {
        try
        {
            var vehicle = await _vehicleRepository.GetByVINAsync(request.VehicleVin)
                          ?? throw new NotFoundException($"Vehicle with ID {request.VehicleVin} not found");

            var auction = await _auctionRepository.GetActiveAuctionByVehicleIdAsync(vehicle.Id)
                ?? throw new NotFoundException($"No Auction was found for Vehicle {request.VehicleVin}");

            if (auction.Status != AuctionStatus.Active)
                throw new InvalidOperationException("Auction is not active");

            if (auction.EndTime <= DateTime.UtcNow)
                throw new InvalidOperationException("Auction has ended");
            var highestBidForVehicle = auction.GetHighestBidForVehicle(vehicle.Id);
            if (request.Amount < highestBidForVehicle.Amount)
                throw new InvalidOperationException(
                    $"Bid must be at least {highestBidForVehicle.Amount}");
            
            var bid = new Bid
            {
                BidderId = request.BidderId,
                Amount = request.Amount,
                BidTime = DateTime.UtcNow,
                AuctionId = auction.Id,
                VehicleId = vehicle.Id
            };
            // auction.PlaceBid(request.BidderId, request.Amount, vehicle.Id);
            await _auctionRepository.AddBidAsync(bid);
            auction = await _auctionRepository.GetByIdAsync(auction.Id);
            _logger.LogInformation("Bid placed successfully for vehicle {VehicleVin}", request.VehicleVin);
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing bid on vehicle: {Vin}", request.VehicleVin);
            throw;
        }
    }

    public async Task<Auction> CancelAuctionAsync(string auctionName)
    {
        try
        {
            var auction = await _auctionRepository.GetAuctionByNameAsync(auctionName)
                          ?? throw new NotFoundException($"Auction with ID {auctionName} not found");

            if (auction.Status == AuctionStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed auction");

            // Release all vehicles
            foreach (var vehicle in auction.Vehicles)
            {
                vehicle.IsAvailable = true;
                await _vehicleRepository.UpdateAsync(vehicle);
            }

            // Remove all bids
            await _auctionRepository.RemoveBidsForAuctionAsync(auction.Id);

            // Update auction status
            auction.Status = AuctionStatus.Cancelled;
            var updatedAuction = await _auctionRepository.UpdateAsync(auction);
        
            _logger.LogInformation("Auction {AuctionId} cancelled successfully", auctionName);
        
            return updatedAuction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling auction: {AuctionId}", auctionName);
            throw;
        }
    }

    public async Task<Auction> CloseAuctionAsync(string auctionName)
    {
        try
        {
            var auction = await _auctionRepository.GetAuctionByNameAsync(auctionName)
                          ?? throw new NotFoundException($"Auction with name {auctionName} not found");

            if (auction.Status != AuctionStatus.Active)
                throw new InvalidOperationException("Can only close active auctions");

            // Set end time to now
            auction.EndTime = DateTime.UtcNow;
            auction.Status = AuctionStatus.Completed;

            // Process winning bids and mark vehicles as sold
            foreach (var vehicle in auction.Vehicles)
            {
                var highestBidder = auction.GetHighestBidderForVehicle(vehicle.Id);
                if (highestBidder != null)
                {
                    // Mark vehicle as sold
                    vehicle.IsAvailable = false;
                    await _vehicleRepository.UpdateAsync(vehicle);
                    
                    // Log the sale
                    await _auctionLogger.LogAuctionCompleted(auction);
                }
                else
                {
                    vehicle.IsAvailable = true;
                }
            }
            
            var updatedAuction = await _auctionRepository.UpdateAsync(auction);
            
            _logger.LogInformation("Auction {AuctionId} closed successfully", auctionName);
            
            return updatedAuction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing auction: {AuctionId}", auctionName);
            throw;
        }
    }

    public async Task<Vehicle> MarkVehicleAsSold(Guid vehicleId, string buyerId)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
        if (vehicle == null)
            throw new InvalidOperationException(
                $"VehicleId {vehicleId} not found");
        var activeAuctions = await _auctionRepository.GetActiveAuctionsAsync();
        var auction = activeAuctions.Where
            (a => a.Vehicles.Any(v => v.Id == vehicleId) && 
                           (a.Status == AuctionStatus.Active || a.Status == AuctionStatus.Scheduled));

        if (auction == null)
            throw new InvalidOperationException(
                $"Active Auction for this vehicle was not found");

        vehicle.BuyerId = buyerId;
        _vehicleRepository.UpdateAsync(vehicle);
        return vehicle;
    }

    public async Task ProcessCompletedAuctionsAsync()
    {
        try
        {
            var completedAuctions = await _auctionRepository
                .GetAuctionsEndingBeforeAsync(DateTime.UtcNow);

            foreach (var auction in completedAuctions)
            {
                try
                {
                    auction.Status = AuctionStatus.Completed;
                    await _auctionRepository.UpdateAsync(auction);

                    var vehiclesWithBids = auction.Vehicles
                        .Where(v => auction.GetHighestBidderForVehicle(v.Id) != null)
                        .ToList();

                    foreach (var vehicle in vehiclesWithBids)
                    {
                        var highestBidderId = auction.GetHighestBidderForVehicle(vehicle.Id);
                        if (highestBidderId == null) continue;
                        await MarkVehicleAsSold(vehicle.Id, highestBidderId);
                        await _auctionLogger.LogAuctionCompleted(auction);
                    }

                    var vehiclesWithoutBids = auction.Vehicles
                        .Except(vehiclesWithBids)
                        .ToList();

                    foreach (var vehicle in vehiclesWithoutBids)
                    {
                        vehicle.IsAvailable = true;
                        await _vehicleRepository.UpdateAsync(vehicle);
                        _logger.LogInformation(
                            "Vehicle {VehicleId} in auction {AuctionId} received no bids", 
                            vehicle.Id, 
                            auction.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error processing completed auction: {AuctionId}", auction.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing completed auctions");
            throw;
        }
    }

    public async Task<Auction> AddVehiclesToAuctionAsync(AddVehiclesToAuctionRequest request)
    {
        try
        {
            var auction = await _auctionRepository.GetAuctionByNameAsync(request.AuctionName)
                          ?? throw new NotFoundException($"Auction with name {request.AuctionName} not found");

            // Get all vehicles requested
            var vehiclesToAdd = new List<Vehicle>();
            foreach (var vin in request.VehicleVins)
            {
                var vehicle = await _vehicleRepository.GetByVINAsync(vin)
                              ?? throw new NotFoundException($"Vehicle with VIN {vin} not found");

                if (!vehicle.IsAvailable)
                    throw new InvalidOperationException($"Vehicle {vin} is not available for auction");

                if (auction.Vehicles.Any(v => v.Id == vehicle.Id))
                    throw new InvalidOperationException($"Vehicle {vin} is already in this auction");

                vehiclesToAdd.Add(vehicle);
            }

            // Mark vehicles as unavailable and add to auction
            foreach (var vehicle in vehiclesToAdd)
            {
                vehicle.IsAvailable = false;
                await _vehicleRepository.UpdateAsync(vehicle);
                auction.Vehicles.Add(vehicle);
            }

            var updatedAuction = await _auctionRepository.UpdateAsync(auction);
            _logger.LogInformation("Added {Count} vehicles to auction {AuctionId}", vehiclesToAdd.Count, request.AuctionName);

            return updatedAuction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicles to auction: {AuctionId}", request.AuctionName);
            throw;
        }
    }


    public async Task<Auction> RemoveVehiclesFromAuctionAsync(RemoveVehiclesFromAuctionRequest request)
    {
        try
        {
            var auction = await _auctionRepository.GetAuctionByNameAsync(request.AuctionName)
                          ?? throw new NotFoundException($"Auction with name {request.AuctionName} not found");
            
            // Verify all vehicles exist in the auction
            foreach (var vehicleVin in request.VehicleVins)
            {
                var vehicle = await _vehicleRepository.GetByVINAsync(vehicleVin)
                              ?? throw new NotFoundException($"Vehicle with VIN {vehicleVin} not found");
            
                // Check if vehicle is in auction
                if (!auction.Vehicles.Any(v => v.Id == vehicle.Id))
                    throw new NotFoundException($"Vehicle {vehicleVin} not found in this auction");
            
                // Check if vehicle has bids
                if (auction.Bids.Any(b => b.VehicleId == vehicle.Id))
                    throw new InvalidOperationException($"Cannot remove vehicle {vehicleVin} as it has existing bids");
            }

            // Remove vehicles from auction and mark them as available
            foreach (var vehicleVin in request.VehicleVins)
            {
                var vehicle = auction.Vehicles.First(v => v.VIN == vehicleVin);
                auction.Vehicles.Remove(vehicle);
            
                vehicle.IsAvailable = true;
                await _vehicleRepository.UpdateAsync(vehicle);
            }

            var updatedAuction = await _auctionRepository.UpdateAsync(auction);
            _logger.LogInformation("Removed {Count} vehicles from auction {AuctionId}", request.VehicleVins.Count(), request.AuctionName);

            return updatedAuction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing vehicles from auction: {AuctionId}", request.AuctionName);
            throw;
        }
    }


    private void ValidateAuctionDto(CreateAuctionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Auction name is required");

        if (request.StartTime >= request.EndTime)
            throw new ValidationException("End time must be after start time");

        if (request.EndTime <= DateTime.UtcNow)
            throw new ValidationException("End time must be in the future");
    }
}