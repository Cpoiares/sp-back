using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using sp_back_api.Database;
using sp_back_api.Database.Repository.Implementation;
using sp_back_api.Services;
using sp_back_api.Services.Implementation;
using sp_back.models.DTOs.Requests;
using sp_back.models.Exceptions;
using sp_back.models.Models.Vehicles;
using sp_back.tests.Helpers;
using Xunit;

namespace sp_back.tests.ServiceTests;

public class VehicleServiceTests : IDisposable
{
    private readonly AuctionDbContext _context;
    private readonly IVehicleService _vehicleService;

    public VehicleServiceTests()
    {
        _context = new AuctionDbContext(TestDatabaseHelper.GetInMemoryDbOptions(Guid.NewGuid().ToString()));
        Mock<ILogger<VehicleService>> loggerMock = new();
        var repository = new VehicleRepository(_context, new Mock<ILogger<VehicleRepository>>().Object);
        _vehicleService = new VehicleService(repository, loggerMock.Object);
    }

    [Fact]
    public async Task CreateSedan_WithValidData_ShouldCreateAndReturnSedan()
    {
        // Arrange
        var request = new CreateVehicleSedanHatchbackRequest("Toyota", "Camry", DateTime.Today.AddMonths(-6), 25000, "SEDAN123", 4);
        
        // Act
        var result = await _vehicleService.CreateSedanAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Sedan>();
        var sedan = (Sedan)result;
        sedan.NumberOfDoors.Should().Be(request.NumberOfDoors);
        result.Make.Should().Be(request.Make);
        result.Model.Should().Be(request.Model);
        result.ProductionDate.Should().Be(request.ProductionDate);
        result.Year.Should().Be(request.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(request.StartingPrice);
        result.IsAvailable.Should().BeTrue();
        result.Vin.Should().Be(request.Vin);
    }

    [Fact]
    public async Task CreateSUV_WithValidData_ShouldCreateAndReturnSUV()
    {
        // Arrange
        var request = new CreateVehicleSuvRequest("Honda", "CR-V", DateTime.Today.AddMonths(-3), 35000,"SUV123", 7);

        // Act
        var result = await _vehicleService.CreateSuvAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Suv>();
        var suv = (Suv)result;
        suv.NumberOfSeats.Should().Be(request.NumberOfSeats);
        result.Make.Should().Be(request.Make);
        result.Model.Should().Be(request.Model);
        result.ProductionDate.Should().Be(request.ProductionDate);
        result.Year.Should().Be(request.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(request.StartingPrice);
        result.IsAvailable.Should().BeTrue();
        result.Vin.Should().Be(request.Vin);
    }

    [Fact]
    public async Task CreateTruck_WithValidData_ShouldCreateAndReturnTruck()
    {
        // Arrange
        var request = new CreateVehicleTruckRequest("Ford", "F-150", DateTime.Today.AddMonths(-1), 45000, "TRUCK123", 1500.5);

        // Act
        var result = await _vehicleService.CreateTruckAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Truck>();
        var truck = (Truck)result;
        truck.LoadCapacity.Should().Be(request.LoadCapacity);
        result.Make.Should().Be(request.Make);
        result.Model.Should().Be(request.Model);
        result.ProductionDate.Should().Be(request.ProductionDate);
        result.Year.Should().Be(request.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(request.StartingPrice);
        result.IsAvailable.Should().BeTrue();
        result.Vin.Should().Be(request.Vin);
    }

    [Fact]
    public async Task CreateHatchback_WithValidData_ShouldCreateAndReturnHatchback()
    {
        // Arrange
        var request = new CreateVehicleSedanHatchbackRequest("Volkswagen", "Golf", DateTime.Today.AddMonths(-2), 30000, "HATCH123", 5);

        // Act
        var result = await _vehicleService.CreateHatchbackAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Hatchback>();
        var hatchback = (Hatchback)result;
        hatchback.NumberOfDoors.Should().Be(request.NumberOfDoors);
        result.Make.Should().Be(request.Make);
        result.Model.Should().Be(request.Model);
        result.ProductionDate.Should().Be(request.ProductionDate);
        result.Year.Should().Be(request.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(request.StartingPrice);
        result.IsAvailable.Should().BeTrue();
        result.Vin.Should().Be(request.Vin);
    }

    [Fact]
    public async Task CreateSedan_WithDuplicateVIN_ShouldThrowException()
    {
        // Arrange
        var request = new CreateVehicleSedanHatchbackRequest("Toyota", "Camry", DateTime.Today, 25000, "DUPLICATE123", 4);

        await _vehicleService.CreateSedanAsync(request);

        // Act
        var act = () => _vehicleService.CreateSedanAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*VIN already exists*");
    }

    [Fact]
    public async Task GetVehicleByVIN_WithNonExistentVIN_ShouldReturnNull()
    {
        // Act
        var act = () => _vehicleService.GetVehicleAsync(1234);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SearchVehicles_WithManufacturerFilter_ShouldReturnMatchingVehicles()
    {
        // Arrange
        await _vehicleService.CreateSedanAsync(new CreateVehicleSedanHatchbackRequest(
            "Toyota", "Camry", DateTime.Today, 25000, "TEST1", 4));
        await _vehicleService.CreateSedanAsync(new CreateVehicleSedanHatchbackRequest(
            "Honda", "Civic", DateTime.Today, 23000, "TEST2", 4));

        // Act
        var result = await _vehicleService.SearchVehiclesAsync(new VehicleSearchParams 
        { 
            Manufacturer = "Toyota" 
        });

        // Assert
        var enumerable = result as Vehicle[] ?? result.ToArray();
        enumerable.Should().HaveCount(1);
        enumerable.First().Make.Should().Be("Toyota");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}