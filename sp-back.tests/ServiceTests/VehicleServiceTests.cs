using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using sp_back_api.Database;
using sp_back_api.Database.Repository.Implementation;
using sp_back_api.DTOs;
using sp_back_api.Services;
using sp_back_api.Services.Implementation;
using sp_back.models.Enums;
using sp_back.models.Exceptions;
using sp_back.models.Models.Vehicles;
using sp_back.tests.Helpers;
using Xunit;

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
    public async Task CreateVehicle_ValidHatchback_ShouldCreateAndReturnHatchback()
    {
        // Arrange
        var dto = new CreateVehicleRequest
        {
            Make = "Hatch",
            Model = "Back",
            NumberOfDoors = 4,
            ProductionDate = DateTime.Today.AddMonths(-6),
            Type = VehicleType.Hatchback,
            StartingPrice = 35000,
            VIN = "1234569786"
        };

        // Act
        var result = await _vehicleService.CreateVehicleAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Hatchback>();
        var sedan = (Hatchback)result;
        sedan.NumberOfDoors.Should().Be(dto.NumberOfDoors);
        result.Make.Should().Be(dto.Make);
        result.Model.Should().Be(dto.Model);
        result.ProductionDate.Should().Be(dto.ProductionDate);
        result.Year.Should().Be(dto.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(dto.StartingPrice);
        result.IsAvailable.Should().BeTrue();
    }
    
    [Fact]
    public async Task CreateVehicle_ValidSedan_ShouldCreateAndReturnSedan()
    {
        // Arrange
        var dto = new CreateVehicleRequest
        {
            Make = "Toyota",
            Model = "Camry",
            NumberOfDoors = 4,
            ProductionDate = DateTime.Today.AddMonths(-6),
            Type = VehicleType.Sedan,
            StartingPrice = 25000,
            VIN = "123456"
        };

        // Act
        var result = await _vehicleService.CreateVehicleAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Sedan>();
        var sedan = (Sedan)result;
        sedan.NumberOfDoors.Should().Be(dto.NumberOfDoors);
        result.Make.Should().Be(dto.Make);
        result.Model.Should().Be(dto.Model);
        result.ProductionDate.Should().Be(dto.ProductionDate);
        result.Year.Should().Be(dto.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(dto.StartingPrice);
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CreateVehicle_ValidSUV_ShouldCreateAndReturnSUV()
    {
        // Arrange
        var dto = new CreateVehicleRequest
        {
            Make = "Honda",
            Model = "CR-V",
            NumberOfSeats = 7,
            ProductionDate = DateTime.Today.AddMonths(-3),
            Type = VehicleType.Suv,
            StartingPrice = 35000,
            VIN = "1234567"
        };

        // Act
        var result = await _vehicleService.CreateVehicleAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SUV>();
        var suv = (SUV)result;
        suv.NumberOfSeats.Should().Be(dto.NumberOfSeats);
        result.Make.Should().Be(dto.Make);
        result.Model.Should().Be(dto.Model);
        result.ProductionDate.Should().Be(dto.ProductionDate);
        result.Year.Should().Be(dto.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(dto.StartingPrice);
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CreateVehicle_ValidTruck_ShouldCreateAndReturnTruck()
    {
        // Arrange
        var dto = new CreateVehicleRequest
        {
            Make = "Ford",
            Model = "F-150",
            LoadCapacity = 1500.5,
            ProductionDate = DateTime.Today.AddMonths(-1),
            Type = VehicleType.Truck,
            StartingPrice = 45000,
            VIN = "12345678"
        };

        // Act
        var result = await _vehicleService.CreateVehicleAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Truck>();
        var truck = (Truck)result;
        truck.LoadCapacity.Should().Be(dto.LoadCapacity);
        result.Make.Should().Be(dto.Make);
        result.Model.Should().Be(dto.Model);
        result.ProductionDate.Should().Be(dto.ProductionDate);
        result.Year.Should().Be(dto.ProductionDate.Year.ToString());
        result.StartingPrice.Should().Be(dto.StartingPrice);
        result.IsAvailable.Should().BeTrue();
    }
    
    

    [Fact]
    public async Task CreateVehicle_TruckWithInvalidLoadCapacity_ShouldThrowValidationException()
    {
        // Arrange
        var dto = new CreateVehicleRequest
        {
            Make = "Ford",
            Model = "F-150",
            LoadCapacity = -100,
            ProductionDate = DateTime.Today.AddMonths(-1),
            Type = VehicleType.Truck,
            StartingPrice = 45000
        };

        // Act
        var act = () => _vehicleService.CreateVehicleAsync(dto);

        // Assert
        await act.Should().ThrowAsync<sp_back.models.Exceptions.ValidationException>();
    }
    

    [Fact]
    public async Task CreateVehicle_MixedProperties_ShouldThrowValidationException()
    {
        // Arrange
        var dto = new CreateVehicleRequest
        {
            Make = "Toyota",
            Model = "Camry",
            NumberOfDoors = 4,
            NumberOfSeats = 5,
            ProductionDate = DateTime.Today.AddMonths(-1),
            Type = VehicleType.Sedan,
            StartingPrice = 25000
        };

        // Act
        var act = () => _vehicleService.CreateVehicleAsync(dto);

        // Assert
        await act.Should().ThrowAsync<sp_back.models.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task UpdateVehicle_ValidSedan_ShouldUpdateAndReturnSedan()
    {
        // Arrange
        var vehicle = await CreateTestSedan();
        var updateDto = new UpdateVehicleRequest
        {
            Id = vehicle.Id,
            Make = "Updated Toyota",
            Model = "Updated Camry",
            NumberOfDoors = 2
        };

        // Act
        var result = await _vehicleService.UpdateVehicleAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Sedan>();
        result.Make.Should().Be(updateDto.Make);
        result.Model.Should().Be(updateDto.Model);
        var sedan = (Sedan)result;
        sedan.NumberOfDoors.Should().Be(updateDto.NumberOfDoors);
    }

    [Fact]
    public async Task UpdateVehicle_ValidSUV_ShouldUpdateAndReturnSUV()
    {
        // Arrange
        var vehicle = await CreateTestSUV();
        var updateDto = new UpdateVehicleRequest
        {
            Id = vehicle.Id,
            Make = "Updated Honda",
            NumberOfSeats = 5
        };

        // Act
        var result = await _vehicleService.UpdateVehicleAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SUV>();
        result.Make.Should().Be(updateDto.Make);
        var suv = (SUV)result;
        suv.NumberOfSeats.Should().Be(updateDto.NumberOfSeats);
    }

    [Fact]
    public async Task UpdateVehicle_ValidTruck_ShouldUpdateAndReturnTruck()
    {
        // Arrange
        var vehicle = await CreateTestTruck();
        var updateDto = new UpdateVehicleRequest
        {
            Id = vehicle.Id,
            LoadCapacity = 2000.00
        };

        // Act
        var result = await _vehicleService.UpdateVehicleAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Truck>();
        var truck = (Truck)result;
        truck.LoadCapacity.Should().Be(updateDto.LoadCapacity);
    }

    [Fact]
    public async Task UpdateVehicle_NonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var updateDto = new UpdateVehicleRequest
        {
            Id = Guid.NewGuid(),
            Make = "Test"
        };

        // Act
        var act = () => _vehicleService.UpdateVehicleAsync(updateDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateVehicle_InvalidTypeChange_ShouldThrowValidationException()
    {
        // Arrange
        var vehicle = await CreateTestSedan();
        var updateDto = new UpdateVehicleRequest
        {
            Id = vehicle.Id,
            Type = VehicleType.Truck
        };

        // Act
        var act = () => _vehicleService.UpdateVehicleAsync(updateDto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
    
    [Fact]
    public async Task UpdateVehicle_ValidTypeChangeToTruck_ShouldUpdateTypeAndProperties()
    {
        // Arrange
        var vehicle = await CreateTestSedan();
        var updateDto = new UpdateVehicleRequest
        {
            Id = vehicle.Id,
            Type = VehicleType.Truck,
            LoadCapacity = 1500.0,
            Make = "Ford",
            Model = "F-150"
        };

        // Act
        var result = await _vehicleService.UpdateVehicleAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Truck>();
        var truck = (Truck)result;
        truck.LoadCapacity.Should().Be(updateDto.LoadCapacity);
        truck.Make.Should().Be(updateDto.Make);
        truck.Model.Should().Be(updateDto.Model);
        truck.Type.Should().Be(VehicleType.Truck);
    }

    [Fact]
    public async Task UpdateVehicle_ValidTypeChangeToSUV_ShouldUpdateTypeAndProperties()
    {
        // Arrange
        var vehicle = await CreateTestSedan();
        var updateDto = new UpdateVehicleRequest
        {
            Id = vehicle.Id,
            Type = VehicleType.Suv,
            NumberOfSeats = 7,
            Make = "Honda",
            Model = "CR-V"
        };

        // Act
        var result = await _vehicleService.UpdateVehicleAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<SUV>();
        var suv = (SUV)result;
        suv.NumberOfSeats.Should().Be(updateDto.NumberOfSeats);
        suv.Make.Should().Be(updateDto.Make);
        suv.Model.Should().Be(updateDto.Model);
        suv.Type.Should().Be(VehicleType.Suv);
    }

    private async Task<Vehicle> CreateTestSedan()
    {
        var dto = new CreateVehicleRequest
        {
            Make = "Toyota",
            Model = "Camry",
            NumberOfDoors = 4,
            ProductionDate = DateTime.Today.AddMonths(-6),
            Type = VehicleType.Sedan,
            StartingPrice = 25000,
            VIN = "TEST123"
        };

        return await _vehicleService.CreateVehicleAsync(dto);
    }

    private async Task<Vehicle> CreateTestSUV()
    {
        var dto = new CreateVehicleRequest
        {
            Make = "Honda",
            Model = "CR-V",
            NumberOfSeats = 7,
            ProductionDate = DateTime.Today.AddMonths(-3),
            Type = VehicleType.Suv,
            StartingPrice = 35000,
            VIN = "TEST456"
        };

        return await _vehicleService.CreateVehicleAsync(dto);
    }

    private async Task<Vehicle> CreateTestTruck()
    {
        var dto = new CreateVehicleRequest
        {
            Make = "Ford",
            Model = "F-150",
            LoadCapacity = 1500.5,
            ProductionDate = DateTime.Today.AddMonths(-1),
            Type = VehicleType.Truck,
            StartingPrice = 45000,
            VIN = "TEST789"
        };

        return await _vehicleService.CreateVehicleAsync(dto);
    }
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}