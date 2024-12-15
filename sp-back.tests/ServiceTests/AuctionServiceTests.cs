using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using sp_back_api.Database;
using sp_back_api.Database.Repository;
using sp_back_api.Database.Repository.Implementation;
using sp_back_api.Logging;
using sp_back_api.Services;
using sp_back_api.Services.Implementation;
using sp_back.models.Config;
using sp_back.models.DTOs.Requests;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Auction;
using sp_back.models.Models.Vehicles;
using sp_back.tests.Helpers;
using Xunit;
using InvalidOperationException = sp_back.models.Exceptions.InvalidOperationException;

namespace sp_back.tests.ServiceTests;

public class AuctionServiceTests : IDisposable
{
    private readonly AuctionDbContext _context;
    private readonly IAuctionService _auctionService;
    private readonly Mock<IAuctionLogger> _auctionLoggerMock;
    private readonly IAuctionRepository _auctionRepository;
    private readonly List<Vehicle> _testVehicles;

    public AuctionServiceTests()
    {
        _context = new AuctionDbContext(TestDatabaseHelper.GetInMemoryDbOptions(Guid.NewGuid().ToString()));
        Mock<ILogger<AuctionService>> loggerMock = new();
        Mock<ILogger<VehicleService>> loggerVehicleMock = new();

        _auctionLoggerMock = new Mock<IAuctionLogger>();


        var auctionRepo = new AuctionRepository(_context, new Mock<ILogger<AuctionRepository>>().Object);
        var vehicleRepo = new VehicleRepository(_context, new Mock<ILogger<VehicleRepository>>().Object);
        Options.Create(new AppSettings());
        _auctionRepository = auctionRepo;
        IVehicleService vehicleService = new VehicleService(vehicleRepo, loggerVehicleMock.Object);

        _auctionService = new AuctionService(
            auctionRepo,
            vehicleRepo,
            _auctionLoggerMock.Object,
            loggerMock.Object,
            vehicleService);

        _testVehicles = SeedTestVehicles();
    }

    private List<Vehicle> SeedTestVehicles()
    {
        var vehicles = new List<Vehicle>
        {
            new Sedan("TestSedan1", "Car1", DateTime.Today.AddMonths(-1), 10000, "VINNUMBER1", 4),
            new Sedan("Test2", "Car2", DateTime.Today.AddMonths(-2), 15000, "VINNUMBER2", 4)
        };

        _context.Vehicles.AddRange(vehicles);
        _context.SaveChanges();
        
        return vehicles;
    }

    [Fact]
    public async Task CreateAuction_WithValidData_ShouldCreateAndReturnAuction()
    {
        // Arrange
        var request = new CreateAuctionRequest(null, _testVehicles.Select(v => v.Vin).ToArray());

        // Act
        var result = await _auctionService.CreateAuctionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Vehicles.Should().HaveCount(2);
        result.Vehicles.Should().Contain(v => _testVehicles.Select(vehicle => v.Id).Contains(v.Id));
    }
    
    [Fact]
    public async Task CreateAuction_WithNonExistentVehicle_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreateAuctionRequest(null, new[] { "testVin1" });

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
        if (vehicle != null) vehicle.IsAvailable = false;
        await _context.SaveChangesAsync();

        var request = new CreateAuctionRequest(null, _testVehicles.Select(v => v.Vin).ToArray());

        // Act
        var act = () => _auctionService.CreateAuctionAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available for auction");
    }

    [Fact]
    public async Task PlaceBid_WithValidBid_ShouldUpdateAuctionWithNewBid()
    {
        // Arrange
        await CreateActiveAuction();
        var vehicle = _testVehicles[0];
        var request = new PlaceBidRequest("testUser", vehicle.Vin, 10500);

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
        var request = new PlaceBidRequest("testUser", _testVehicles[0].Vin, 10500);

        // Act
        var act = () => _auctionService.PlaceBidAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task PlaceBid_OnCompletedAuction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        await CreateCompletedAuction();
        var request = new PlaceBidRequest("testUser", _testVehicles[0].Vin, 10500);

        // Act
        var act = () => _auctionService.PlaceBidAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private async Task CreateActiveAuction(DateTime? endTime = null)
    {
        var auction = new Auction
        {
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = endTime ?? DateTime.UtcNow.AddDays(1),
            Status = AuctionStatus.Active,
            Vehicles = _testVehicles
        };

        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }
    
    [Fact]
    public async Task CancelAuction_ShouldReleaseVehiclesAndRemoveBids()
    {
        // Arrange
        var auction = await CreateActiveAuctionWithWinnerBid();

        // Act
        var result = await _auctionService.CancelAuctionAsync(auction.Id);

        // Assert
        result.Status.Should().Be(AuctionStatus.Cancelled);
        result.Bids.Should().BeEmpty();

        // Verify vehicles are released
        foreach (var vehicle in result.Vehicles)
        {
            var updatedVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
            updatedVehicle?.IsAvailable.Should().BeTrue();
        }
    }
    
    [Fact]
    public async Task CancelAuction_CompletedAuction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var auction = await CreateCompletedAuction();

        // Act
        var act = () => _auctionService.CancelAuctionAsync(auction.Id);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Cannot cancel a completed auction");
    }

    [Fact]
    public async Task CloseAuction_ShouldProcessWinnersAndMarkVehiclesAsSold()
    {
        // Arrange
        var auction = await CreateActiveAuctionWithWinnerBid();

        // Act
        var result = await _auctionService.CloseAuctionAsync(auction.Id);

        // Assert
        result.Status.Should().Be(AuctionStatus.Completed);
        result.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify vehicles with winning bids are marked as sold
        var vehicleWithBid = await _context.Vehicles.FindAsync(_testVehicles[0].Id);
        vehicleWithBid?.IsAvailable.Should().BeFalse();

        // Verify vehicles without bids remain available
        var vehicleWithoutBid = await _context.Vehicles.FindAsync(_testVehicles[1].Id);
        vehicleWithoutBid?.IsAvailable.Should().BeTrue();

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
        var act = async () => await _auctionService.CloseAuctionAsync(auction.Id);
    
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Can only close active auctions");
    }
    
    [Fact]
    public async Task CreateCollectiveAuction_WithValidData_ShouldCreateAuctionWithSameBidForAllVehicles()
    {
        // Arrange
        var startingBid = 5000.0;
        var request = new CreateCollectiveAuctionRequest(null, _testVehicles.Select(v => v.Vin).ToArray(), startingBid);

        // Act
        var result = await _auctionService.CreateCollectiveAuction(request);

        // Assert
        result.Should().NotBeNull();
        result.IsCollectiveAuction.Should().BeTrue();
        result.Status.Should().Be(AuctionStatus.Waiting);
        result.Vehicles.Should().HaveCount(_testVehicles.Count);
        result.Vehicles.Should().OnlyContain(v => Math.Abs(v.StartingPrice - startingBid) < 0.0001);
    }
    
    [Fact]
    public async Task PlaceBidInCollectiveAuction_WithValidBid_ShouldUpdateAllVehiclesWithSameBid()
    {
        // Arrange
        var auction = await CreateTestCollectiveAuction();
        var bidAmount = 15000.0;
        var request = new PlaceBidInCollectiveAuctionRequest("testBidder", auction.Id, bidAmount);

        // Act
        var result = await _auctionService.PlaceBidInCollectiveAuction(request);

        // Assert
        result.Should().NotBeNull();
        result.Bids.Should().HaveCount(auction.Vehicles.Count);
        foreach (var vehicle in result.Vehicles)
        {
            var highestBid = result.GetHighestBidForVehicle(vehicle.Id);
            highestBid.Should().NotBeNull();
            highestBid.Amount.Should().Be(bidAmount);
            highestBid.BidderId.Should().Be(request.BidderId);
        }
    }
    
    [Fact]
    public async Task PlaceBidInCollectiveAuction_WithLowerBid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var auction = await CreateTestCollectiveAuction();
        // Place an initial bid
        await _auctionService.PlaceBidInCollectiveAuction(new PlaceBidInCollectiveAuctionRequest("firstBidder", auction.Id, 15000.0));

        var lowerBidRequest = new PlaceBidInCollectiveAuctionRequest("secondBidder", auction.Id, 14000.0);

        // Act
        var act = () => _auctionService.PlaceBidInCollectiveAuction(lowerBidRequest);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Bid must be at least*");
    }
    
    private async Task<Auction> CreateActiveAuctionWithWinnerBid(DateTime? endTime = null)
    {
        // First create and save the auction with vehicles
        var auction = new Auction
        {
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

        var bid = new Bid("winner1", vehicles[0], auction, 99999999999.0);

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync();
    
        return await _context.Auctions
            .Include(a => a.Vehicles)
            .Include(a => a.Bids)
            .FirstAsync(a => a.Id == auction.Id);
    }
    
    private async Task<Auction> CreateTestCollectiveAuction()
    {
        var request = new CreateCollectiveAuctionRequest(DateTime.UtcNow.AddDays(1), _testVehicles.Select(v => v.Vin).ToArray(), 10000.0);

        var auction = await _auctionService.CreateCollectiveAuction(request);
        auction.Status = AuctionStatus.Active;
        await _auctionRepository.UpdateAsync(auction);
    
        return auction;
    }
    
    private async Task<Auction> CreateCompletedAuction()
    {
        var auction = new Auction
        {
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