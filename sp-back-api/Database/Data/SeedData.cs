using sp_back.models.Enums;
using sp_back.models.Models.Auction;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Database.Data;

public static class SeedData
{
    public static IEnumerable<Vehicle> GetVehicles()
    {
        return new Vehicle[]
        {
            new Hatchback()
            {
                Id = Guid.Parse("c5ae495d-9bd7-4256-981c-931d2b3bd999"),
                Make = "Mazda",
                Model = "Hatchback",
                NumberOfDoors = 4,
                ProductionDate = DateTime.Today,
                Type = VehicleType.Hatchback,
                StartingPrice = 35000,
                IsAvailable = false,
                VIN = "14576889"

            },
            new Sedan()
            {
                Id = Guid.Parse("c5ae495d-9bd7-4256-981c-931d2b3bd947"),
                Make = "Toyota",
                Model = "Camry",
                NumberOfDoors = 10,
                ProductionDate = DateTime.Today,
                Type = VehicleType.Sedan,
                StartingPrice = 25000,
                IsAvailable = true,
                VIN = "1"
            },
            new Suv()
            {
                Id = Guid.Parse("3f00cb54-1534-4f23-b35f-4c3fb1e1c110"),
                Make = "Ford",
                Model = "Explorer",
                NumberOfSeats = 10,
                ProductionDate = DateTime.Today.AddMonths(-67),
                Type = VehicleType.Suv,
                StartingPrice = 35000,
                IsAvailable = true,
                VIN = "2"
            },
            new Truck
            {
                Id = Guid.Parse("d6ed42ce-e35b-4723-8174-83c26457c5d8"),
                Make = "Chevrolet",
                Model = "Silverado",
                LoadCapacity = 10,
                ProductionDate = DateTime.Today.AddMonths(-37),
                Type = VehicleType.Truck,
                StartingPrice = 45000,
                IsAvailable = true,
                VIN = "3"
            },
            new Sedan
            {
                Id = Guid.Parse("8b5e7a89-ef1d-4a07-9425-c884b0c148ab"),
                Make = "Honda",
                Model = "Civic",
                NumberOfDoors = 4,
                ProductionDate = DateTime.Today.AddMonths(-27),
                Type = VehicleType.Sedan,
                StartingPrice = 28000,
                IsAvailable = false,
                VIN = "4"
            },
            new Suv
            {
                Id = Guid.Parse("1a563e5f-7ce9-4c0f-a0f4-9e23a4e1b838"),
                Make = "Tesla",
                Model = "Model Y",
                NumberOfSeats = 4,
                ProductionDate = DateTime.Today.AddMonths(-17),
                Type = VehicleType.Suv,
                StartingPrice = 55000,
                IsAvailable = false,
                VIN = "5"
            }
        };
    }

    public static IEnumerable<Auction> GetAuctions()
    {
        return new[]
        {
            new Auction
            {
                Id = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"),
                Name = "Premium Auction",
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddMinutes(-1),
                Status = AuctionStatus.Active,
            },
            new Auction
            {
                Id = Guid.Parse("1e43c7e7-2e7e-4c5a-9a6d-8c9b25c5c556"),
                Name = "Luxury Special",
                Status = AuctionStatus.Waiting,
            },
            new Auction
            {
                Id = Guid.Parse("9d2e6f0e-b5a2-4c8f-b3e4-3f2e6a3b123c"),
                Name = "Upcoming Auction",
                StartTime = DateTime.UtcNow,
                Status = AuctionStatus.Waiting,

            }
        };
    }

    public static IEnumerable<Bid> GetBids()
    {
        return new[]
        {
            new Bid
            {
                Id = Guid.NewGuid(),
                BidderId = "winner123",
                Amount = 40000.0,
                BidTime = DateTime.UtcNow.AddHours(-1),
                AuctionId = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"), // First auction ID
                VehicleId = Guid.Parse("c5ae495d-9bd7-4256-981c-931d2b3bd999")  // Mazda's ID
            },
            new Bid
            {
                Id = Guid.NewGuid(),
                BidderId = "winner1235",
                Amount = 30000.0,
                BidTime = DateTime.UtcNow.AddMinutes(-30),
                AuctionId = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"),
                VehicleId = Guid.Parse("8b5e7a89-ef1d-4a07-9425-c884b0c148ab")  // Honda's ID
            }
        };
    }
}