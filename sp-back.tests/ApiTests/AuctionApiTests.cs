using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using sp_back_api.Database;
using sp_back_api.DTOs;
using sp_back.models.Enums;
using sp_back.models.Models.Auction;
using sp_back.models.Models.Vehicles;
using Xunit;

namespace sp_back.tests.ApiTests;

public class AuctionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _databaseName;

    public AuctionApiTests(WebApplicationFactory<Program> factory)
    {
        _databaseName = "AuctionDb";
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
                
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<AuctionDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        });

        _client = _factory.CreateClient();
    }

    // Vehicle Tests
    [Fact]
    public async Task GetVehicles_ShouldReturnSuccessAndVehiclesList()
    {
        // Act
        var response = await _client.GetAsync("/vehicles");
        var vehicles = await response.Content.ReadFromJsonAsync<IEnumerable<Vehicle>>();

        // Assert
        response.Should().BeSuccessful();
        vehicles.Should().NotBeNull();
    }

    [Fact]
    public async Task GetVehicle_WithValidId_ShouldReturnVehicle()
    {
        // Arrange
        var createdVehicle = await CreateTestVehicle();

        // Act
        var response = await _client.GetAsync($"/vehicles/{createdVehicle.Id}");
        var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>();

        // Assert
        response.Should().BeSuccessful();
        vehicle.Should().NotBeNull();
        vehicle.Id.Should().Be(createdVehicle.Id);
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ShouldReturnCreatedAndNewVehicle()
    {
        // Arrange
        var vehicle = new CreateVehicleRequest
        {
            Make = "Toyota",
            Model = "Camry",
            NumberOfDoors = 4,
            ProductionDate = DateTime.Today.AddMonths(-1),
            Type = VehicleType.Sedan,
            VIN = "123456",
            StartingPrice = 25000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/vehicles", vehicle);
        var result = await response.Content.ReadFromJsonAsync<Vehicle>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result.Make.Should().Be(vehicle.Make);
        result.VIN.Should().Be(vehicle.VIN);
    }

    // Auction Tests
    [Fact]
    public async Task CreateAuction_WithValidData_ShouldReturnCreatedAuction()
    {
        // Arrange
        var vehicle = await CreateTestVehicle();
        var request = new CreateAuctionRequest
        {
            Name = "Test Auction",
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddDays(1),
            VehicleVins = new[] { vehicle.VIN }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auctions", request);
        var result = await response.Content.ReadFromJsonAsync<Auction>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Vehicles.Should().Contain(v => v.VIN == vehicle.VIN);
    }

    [Fact]
    public async Task GetActiveAuctions_ShouldReturnOnlyActiveAuctions()
    {
        // Arrange
        await CreateTestAuction(AuctionStatus.Active);
        await CreateTestAuction(AuctionStatus.Completed);

        // Act
        var response = await _client.GetAsync("/auctions/active");
        var auctions = await response.Content.ReadFromJsonAsync<IEnumerable<Auction>>();

        // Assert
        response.Should().BeSuccessful();
        auctions.Should().NotBeNull();
        auctions.Should().OnlyContain(a => a.Status == AuctionStatus.Active);
    }

    [Fact]
    public async Task PlaceBid_WithValidData_ShouldUpdateAuction()
    {
        // Arrange
        var auction = await CreateTestAuction(AuctionStatus.Active);
        var request = new PlaceBidRequest
        {
            BidderId = "testUser",
            Amount = 30000,
            VehicleVin = auction.Vehicles.First().VIN
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auctions/bid", request);
        var result = await response.Content.ReadFromJsonAsync<Auction>();

        // Assert
        response.Should().BeSuccessful();
        result.Should().NotBeNull();
        result.Bids.Should().Contain(b => b.Amount == request.Amount);
    }

    [Fact]
    public async Task CancelAuction_ShouldUpdateAuctionStatus()
    {
        // Arrange
        var auction = await CreateTestAuction(AuctionStatus.Active);

        // Act
        var response = await _client.PostAsync($"/auctions/{auction.Id}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<Auction>();

        // Assert
        response.Should().BeSuccessful();
        result.Should().NotBeNull();
        result.Status.Should().Be(AuctionStatus.Cancelled);
    }

    [Fact]
    public async Task AddVehiclesToAuction_WithValidData_ShouldUpdateAuction()
    {
        // Arrange
        var auction = await CreateTestAuction(AuctionStatus.Scheduled);
        var newVehicle = await CreateTestVehicle();
        var request = new AddVehiclesToAuctionRequest
        {
            VehicleVins = new[] { newVehicle.VIN }
        };

        // Act
        var response = await _client.PostAsync(
            $"/auctions/{auction.Id}/vehicles",
            JsonContent.Create(request));
        var result = await response.Content.ReadFromJsonAsync<Auction>();

        // Assert
        response.Should().BeSuccessful();
        result.Should().NotBeNull();
        result.Vehicles.Should().Contain(v => v.VIN == newVehicle.VIN);
    }

    // Helper Methods
    private async Task<Vehicle> CreateTestVehicle()
    {
        var request = new CreateVehicleRequest
        {
            Make = "Test",
            Model = "Car",
            NumberOfDoors = 4,
            ProductionDate = DateTime.Today.AddMonths(-1),
            Type = VehicleType.Sedan,
            VIN = "1234567777",
            StartingPrice = 25000
        };

        var response = await _client.PostAsJsonAsync("/vehicles", request);
        return await response.Content.ReadFromJsonAsync<Vehicle>();
    }

    private async Task<Auction> CreateTestAuction(AuctionStatus status)
    {
        var vehicle = await CreateTestVehicle();
        
        // Verify vehicle was created
        var vehicleResponse = await _client.GetAsync($"/vehicles/{vehicle.VIN}");
        vehicleResponse.Should().BeSuccessful($"Vehicle with VIN {vehicle.VIN} should exist");
        
        var request = new CreateAuctionRequest
        {
            
            Name = "Test Auction",
            StartTime = status == AuctionStatus.Active ? 
                DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddDays(1),
            VehicleVins = new[] { vehicle.VIN }
        };

        var response = await _client.PostAsJsonAsync("/auctions", request);
        return await response.Content.ReadFromJsonAsync<Auction>();
    }
}