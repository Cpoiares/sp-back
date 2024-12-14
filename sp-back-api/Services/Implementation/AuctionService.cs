using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sp_back_api.Database.Repository;
using sp_back_api.DTOs;
using sp_back_api.Loging;
using sp_back.models.Config;
using sp_back.models.DTOs.Requests;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Auction;
using sp_back.models.Models.Vehicles;

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
                Name = request.Name,
                Vehicles = auctionVehicles,
                Status = AuctionStatus.Waiting,
                EndTime = request.EndDate,
            };



            await _vehicleService.LockVehicleInAuction(auctionVehicles);
            return await _auctionRepository.AddAsync(auction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auction with vehicles: {VehicleId}",
                string.Join(',', request.VehicleVins));
            throw;
        }
    }

    public async Task<Auction> StartAuctionAsync(string auctionName)
    {
        try
        {
            var a = await _auctionRepository.GetAuctionByNameAsync(auctionName);

            if (a == null)
                throw new NotFoundException($"Auction with name {auctionName} not found");

            if (a.Status != AuctionStatus.Waiting)
                throw new ValidationException(
                    "Cannot start an auction that is cancelled, in progress or already completed.");


            var auction = await _auctionRepository.StartAuction(auctionName);
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting auction: {auctionName}", auctionName);
            throw;
        }
    }

    public async Task<Auction> CreateCollectiveAuction(CreateCollectiveAuctionRequest request)
    {
        try
        {
            var auctionVehicles = new List<Vehicle>();

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

                vehicle.StartingPrice = request.StartingBid;
                auctionVehicles.Add(vehicle);
            }

            var auction = new Auction
            {
                Name = request.AuctionName,
                Vehicles = auctionVehicles,
                Status = AuctionStatus.Waiting,
                IsCollectiveAuction = true,
                EndTime = request.EndDate,
            };



            await _vehicleService.LockVehicleInAuction(auctionVehicles);
            return await _auctionRepository.AddAsync(auction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auction with vehicles: {VehicleId}",
                string.Join(',', request.VehicleVins));
            throw;
        }
    }

    public async Task<Auction> PlaceBidInCollectiveAuction(PlaceBidInCollectiveAuctionRequest request)
    {
        try
        {
            var auction = await _auctionRepository.GetAuctionByNameAsync(request.AuctionName)
                          ?? throw new NotFoundException($"No Auction was found for name {request.AuctionName}");

            if (auction.Status != AuctionStatus.Active)
                throw new ValidationException("Auction is not active");

            if (!auction.IsCollectiveAuction)
                throw new ValidationException("Auction is not collective auction");
            
            if (auction.EndTime <= DateTime.UtcNow)
                throw new ValidationException("Auction has ended");
            
            if(!auction.Vehicles.Any())
                throw new ValidationException($"No vehicles are available in auction {auction.Name}");
            
            var highestBidForVehicle = auction.GetHighestBidForVehicle(auction.Vehicles.First().Id);
            
            if (request.Amount < highestBidForVehicle.Amount)
                throw new ValidationException(
                    $"Bid must be at least {highestBidForVehicle.Amount}");
            
            foreach (var vehicle in auction.Vehicles)
            {
                var bid = new Bid
                {
                    BidderId = request.BidderId,
                    Amount = request.Amount,
                    BidTime = DateTime.UtcNow,
                    AuctionId = auction.Id,
                    VehicleId = vehicle.Id
                };

                await _auctionRepository.AddBidAsync(bid);
            }
            
            auction = await _auctionRepository.GetByIdAsync(auction.Id);
            _logger.LogInformation("Bid placed successfully on all vehicles of auction {auctionName}", request.AuctionName);
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing bid on all vehicles of auction: {auctionName}", request.AuctionName);
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
                throw new ValidationException("Auction is not active");
            
            if (auction.IsCollectiveAuction)
                throw new ValidationException("Auction is not collective auction, individual bids on vehicles cannot be placed");

            if (auction.EndTime <= DateTime.UtcNow)
                throw new ValidationException("Auction has ended");
            var highestBidForVehicle = auction.GetHighestBidForVehicle(vehicle.Id);
            if (request.Amount < highestBidForVehicle.Amount)
                throw new ValidationException(
                    $"Bid must be at least {highestBidForVehicle.Amount}");
            
            var bid = new Bid
            {
                BidderId = request.BidderId,
                Amount = request.Amount,
                BidTime = DateTime.UtcNow,
                AuctionId = auction.Id,
                VehicleId = vehicle.Id
            };
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
                throw new ValidationException("Cannot cancel a completed auction");

            // Release all vehicles
            foreach (var vehicle in auction.Vehicles.ToList())
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
                throw new ValidationException("Can only close active auctions");

            var auctionVehicles = auction.Vehicles.ToList();
            foreach (var vehicle in auctionVehicles)
            {
                var highestBidder = auction.GetHighestBidderForVehicle(vehicle.Id);
                if (highestBidder != null)
                {
                    await MarkVehicleAsSold(vehicle.Id, highestBidder);
                    await _auctionLogger.LogAuctionCompleted(auction);
                }
            }
            
            var vehiclesWithoutBids = auction.Vehicles
                .Where(v => string.IsNullOrEmpty(auction.GetHighestBidderForVehicle(v.Id)))
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
            await _auctionRepository.CloseAuctionAsync(auction.Id);
            
            _logger.LogInformation("Auction {AuctionId} closed successfully", auctionName);
            
            return auction;
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
            throw new NotFoundException(
                $"VehicleId {vehicleId} not found");
        
        var activeAuctions = await _auctionRepository.GetActiveAuctionsAsync();
        
        var auction = activeAuctions.Where
            (a => a.Vehicles.Any(v => v.Id == vehicleId) && 
                           (a.Status == AuctionStatus.Active));

        if (auction == null)
            throw new NotFoundException(
                $"Active Auction for this vehicle was not found");
        
        await _vehicleRepository.MarkVehicleAsSold(vehicle.Id, buyerId);
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
                    }
                    
                    await _auctionLogger.LogAuctionCompleted(auction);

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
            
            if (auction.Status != AuctionStatus.Active)
                throw new ValidationException("Can only add vehicles to active auctions");
            
            var vehiclesToAdd = new List<Vehicle>();
            foreach (var vin in request.VehicleVins)
            {
                var vehicle = await _vehicleRepository.GetByVINAsync(vin)
                              ?? throw new NotFoundException($"Vehicle with VIN {vin} not found");

                if (!vehicle.IsAvailable)
                    throw new ValidationException($"Vehicle {vin} is not available for auction");

                if (auction.Vehicles.Any(v => v.Id == vehicle.Id))
                    throw new ValidationException($"Vehicle {vin} is already in this auction");

                vehiclesToAdd.Add(vehicle);
            }

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
            
            if (auction.Status != AuctionStatus.Active)
                throw new ValidationException("Can only remove vehicles from active auctions");
            
            foreach (var vehicleVin in request.VehicleVins)
            {
                var vehicle = await _vehicleRepository.GetByVINAsync(vehicleVin)
                              ?? throw new NotFoundException($"Vehicle with VIN {vehicleVin} not found");
            
                if (!auction.Vehicles.Any(v => v.Id == vehicle.Id))
                    throw new NotFoundException($"Vehicle {vehicleVin} not found in this auction");
            
                if (auction.Bids.Any(b => b.VehicleId == vehicle.Id))
                    throw new ValidationException($"Cannot remove vehicle {vehicleVin} as it has existing bids");
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

    public async Task<IEnumerable<Auction>> GetAllAuctionsAsync()
    {
        try
        {
            return await _auctionRepository.GetAllAuctionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active auctions");
            throw;
        }
    }
}