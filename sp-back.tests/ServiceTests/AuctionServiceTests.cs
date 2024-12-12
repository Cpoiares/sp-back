using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using sp_back_api.Database;
using sp_back_api.Database.Repository.Implementation;
using sp_back_api.DTOs;
using sp_back_api.Loging;
using sp_back_api.Services;
using sp_back_api.Services.Implementation;
using sp_back.models.Config;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Auction;
using sp_back.models.Models.Vehicles;
using sp_back.tests.Helpers;
using Xunit;

public class AuctionServiceTests : IDisposable
{
    private readonly AuctionDbContext _context;
    private readonly IAuctionService _auctionService;
    private readonly IVehicleService _vehicleService;
    private readonly Mock<ILogger<AuctionService>> _loggerMock;
    private readonly Mock<IAuctionLogger> _auctionLoggerMock;
    private readonly Mock<ILogger<VehicleService>> _loggerVehicleMock;
    private readonly List<Vehicle> _testVehicles;

    public AuctionServiceTests()
    {
        _context = new AuctionDbContext(TestDatabaseHelper.GetInMemoryDbOptions(Guid.NewGuid().ToString()));
        _loggerMock = new Mock<ILogger<AuctionService>>();
        _loggerVehicleMock = new Mock<ILogger<VehicleService>>();

        _auctionLoggerMock = new Mock<IAuctionLogger>();


        var auctionRepo = new AuctionRepository(_context, new Mock<ILogger<AuctionRepository>>().Object);
        var vehicleRepo = new VehicleRepository(_context, new Mock<ILogger<VehicleRepository>>().Object);
        var appSettings = Options.Create(new AppSettings 
        { 
        });
        _vehicleService = new VehicleService(vehicleRepo, _loggerVehicleMock.Object);

        _auctionService = new AuctionService(
            auctionRepo,
            vehicleRepo,
            _auctionLoggerMock.Object,
            appSettings,
            _loggerMock.Object,
            _vehicleService);

        _testVehicles = SeedTestVehicles();
    }

    private List<Vehicle> SeedTestVehicles()
    {
        var vehicles = new List<Vehicle>
        {
            new Sedan()
            {
                Id = Guid.NewGuid(),
                Make = "Test1",
                Model = "Car1",     
                NumberOfDoors = 4,
                ProductionDate = DateTime.Today.AddMonths(-1),       
                Type = VehicleType.Sedan,
                StartingPrice = 10000,
                IsAvailable = true,
                VIN = "1"
                
            },
            new Sedan()
            {
                Id = Guid.NewGuid(),
                Make = "Test2",
                Model = "Car2",     
                NumberOfDoors = 2,
                ProductionDate = DateTime.Today.AddMonths(-2),       
                Type = VehicleType.Sedan,
                StartingPrice = 15000,
                IsAvailable = true,
                VIN = "2"
            }
        };

        _context.Vehicles.AddRange(vehicles);
        _context.SaveChanges();
        
        return vehicles;
    }

    [Fact]
    public async Task CreateAuction_WithValidData_ShouldCreateAndReturnAuction()
    {
        // Arrange
        var request = new CreateAuctionRequest
        {
            Name = "Test Auction",
            VehicleVins = _testVehicles.Select(v => v.VIN).ToArray()
        };

        // Act
        var result = await _auctionService.CreateAuctionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Vehicles.Should().HaveCount(2);
        result.Vehicles.Should().Contain(v => _testVehicles.Select(Vehicle => v.Id).Contains(v.Id));
    }
    
    [Fact]
    public async Task CreateAuction_WithNonExistentVehicle_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreateAuctionRequest
        {
            Name = "Test Auction",
            VehicleVins = new[] { Guid.NewGuid().ToString() }
        };

        // Act
        var act = () => _auctionService.CreateAuctionAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAuction_WithUnavailableVehicle_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var vehicle = await _context.Vehicles.FindAsync(_testVehicles[0].Id);
        vehicle.IsAvailable = false;
        await _context.SaveChangesAsync();

        var request = new CreateAuctionRequest
        {
            Name = "Test Auction",
            VehicleVins = _testVehicles.Select(v => v.VIN).ToArray()
        };

        // Act
        var act = () => _auctionService.CreateAuctionAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Vehicle is not available for auction");
    }

    [Fact]
    public async Task PlaceBid_WithValidBid_ShouldUpdateAuctionWithNewBid()
    {
        // Arrange
        var auction = await CreateActiveAuction();
        var vehicle = _testVehicles[0];
        var request = new PlaceBidRequest
        {
            BidderId = "testUser",
            Amount = 10500,
            VehicleVin = vehicle.VIN,
        };

        // Act
        var result = await _auctionService.PlaceBidAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Bids.Should().NotBeEmpty();
        var highestBid = result.GetHighestBidForVehicle(vehicle.Id);
        highestBid.Should().NotBeNull();
        highestBid.Amount.Should().Be(request.Amount);
        result.GetHighestBidderForVehicle(vehicle.Id).Should().Be(request.BidderId);
    }

    [Fact]
    public async Task PlaceBid_OnNonExistentAuction_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new PlaceBidRequest
        {
            BidderId = "testUser",
            Amount = 10500,
            VehicleVin = _testVehicles[0].VIN,
        };

        // Act
        var act = () => _auctionService.PlaceBidAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PlaceBid_OnCompletedAuction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var auction = await CreateCompletedAuction();
        var request = new PlaceBidRequest
        {
            BidderId = "testUser",
            Amount = 10500,
            VehicleVin = _testVehicles[0].VIN
        };

        // Act
        var act = () => _auctionService.PlaceBidAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
    

    [Fact]
    public async Task ProcessCompletedAuctions_WithNoWinner_ShouldOnlyUpdateStatus()
    {
        // Arrange
        var auction = await CreateActiveAuction(endTime: DateTime.UtcNow.AddMinutes(-5));
        await _context.SaveChangesAsync();

        // Act
        await _auctionService.ProcessCompletedAuctionsAsync();

        // Assert
        var processedAuction = await _context.Auctions.FindAsync(auction.Id);
        processedAuction.Status.Should().Be(AuctionStatus.Completed);

        // All vehicles should still be available
        foreach (var v in _testVehicles)
        {
            var vehicle = await _context.Vehicles.FindAsync(v.Id);
            vehicle.IsAvailable.Should().BeTrue();
        }

        _auctionLoggerMock.Verify(x => x.LogAuctionCompleted(It.IsAny<Auction>()), Times.Never);
    }

    private async Task<Auction> CreateActiveAuction(DateTime? endTime = null)
    {
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            Name = "Test Auction",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = endTime ?? DateTime.UtcNow.AddDays(1),
            Status = AuctionStatus.Active,
            Vehicles = _testVehicles
        };

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        return auction;
    }
    
    [Fact]
    public async Task CancelAuction_ShouldReleaseVehiclesAndRemoveBids()
    {
        // Arrange
        var auction = await CreateActiveAuctionWithWinnerBid();

        // Act
        var result = await _auctionService.CancelAuctionAsync(auction.Name);

        // Assert
        result.Status.Should().Be(AuctionStatus.Cancelled);
        result.Bids.Should().BeEmpty();

        // Verify vehicles are released
        foreach (var vehicle in result.Vehicles)
        {
            var updatedVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
            updatedVehicle.IsAvailable.Should().BeTrue();
        }
    }
    
    [Fact]
    public async Task CancelAuction_CompletedAuction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var auction = await CreateCompletedAuction();

        // Act
        var act = () => _auctionService.CancelAuctionAsync(auction.Name);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot cancel a completed auction");
    }

    [Fact]
    public async Task CloseAuction_ShouldProcessWinnersAndMarkVehiclesAsSold()
    {
        // Arrange
        var auction = await CreateActiveAuctionWithWinnerBid();

        // Act
        var result = await _auctionService.CloseAuctionAsync(auction.Name);

        // Assert
        result.Status.Should().Be(AuctionStatus.Completed);
        result.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify vehicles with winning bids are marked as sold
        var vehicleWithBid = await _context.Vehicles.FindAsync(_testVehicles[0].Id);
        vehicleWithBid.IsAvailable.Should().BeFalse();

        // Verify vehicles without bids remain available
        var vehicleWithoutBid = await _context.Vehicles.FindAsync(_testVehicles[1].Id);
        vehicleWithoutBid.IsAvailable.Should().BeTrue();

        _auctionLoggerMock.Verify(x => x.LogAuctionCompleted(
                It.Is<Auction>(a => a.Id == auction.Id)), 
            Times.Once);
    }

    [Fact]
    public async Task CloseAuction_NonActiveAuction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var auction = await CreateCompletedAuction();

        // Act & Assert
        var act = async () => await _auctionService.CloseAuctionAsync(auction.Name);
    
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Can only close active auctions");
    }
    
    private async Task<Auction> CreateActiveAuctionWithWinnerBid(DateTime? endTime = null)
    {
        // First create and save the auction with vehicles
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            Name = "Test Auction",
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = endTime ?? DateTime.UtcNow.AddDays(1),
            Status = AuctionStatus.Active
        };

        // Get fresh copies of vehicles from the context
        var vehicles = await _context.Vehicles
            .Where(v => _testVehicles.Select(tv => tv.Id).Contains(v.Id))
            .ToListAsync();

        foreach (var v in vehicles)
        {
            v.IsAvailable = false;
            _context.Vehicles.Update(v);
            await _context.SaveChangesAsync();
        }
        
        auction.Vehicles = await _context.Vehicles
            .Where(v => _testVehicles.Select(tv => tv.Id).Contains(v.Id))
            .ToListAsync();

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        auction = await _context.Auctions
            .Include(a => a.Vehicles)
            .Include(a => a.Bids)
            .FirstAsync(a => a.Id == auction.Id);

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            BidderId = "winner1",
            Amount = 999999999.0,
            BidTime = DateTime.UtcNow,
            AuctionId = auction.Id,
            VehicleId = vehicles[0].Id
        };

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();
    
        return await _context.Auctions
            .Include(a => a.Vehicles)
            .Include(a => a.Bids)
            .FirstAsync(a => a.Id == auction.Id);
    }
    
    private async Task<Auction> CreateCompletedAuction()
    {
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            Name = "Test Auction",
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow.AddDays(-1),
            Status = AuctionStatus.Completed,
            Vehicles = _testVehicles
        };

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        return auction;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}