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
            new Hatchback("Mazda", "Hatchback", DateTime.Today, 35000, "VINNUMBER1", 4)
            {
                Id = 1,
                IsAvailable = false
            },
            new Sedan("Toyota", "Camry" , DateTime.Today, 25000, "VINNUMBER2", 10)
            {
                Id = 2,
            },
            new Suv("Ford", "Explorer", DateTime.Today.AddMonths(-67), 35000 ,"VINNUMBER3", 10)            
            {
                Id = 3,
            },
            new Truck("Chevrolet", "Silverado", DateTime.Today.AddMonths(-37), 45000, "VINNUMBER4",100)            
            {
                Id = 4,
            },
            new Sedan("Honda", "Civic", DateTime.Today.AddMonths(-27), 28000,"VINNUMBER5", 4)            
            {
                Id = 5,
                IsAvailable = false
            },
            new Suv("Tesla", "Model Y", DateTime.Today.AddMonths(-18), 55000, "VINNUMBER6", 4)
            {
                Id = 6,
            },
        };
    }

    public static IEnumerable<Auction> GetAuctions()
    {
        return new[]
        {
            new Auction(DateTime.UtcNow.AddMinutes(-1))
            {
                Id = 1,
                StartTime = DateTime.UtcNow.AddDays(-2),
                Status = AuctionStatus.Active,
            },
            new Auction( null)
            {
                Id = 2,
            } ,
            new Auction(null)
            {
                Id = 3,
                StartTime = DateTime.UtcNow,
                Status = AuctionStatus.Active,
            }
        };
    }

    public static IEnumerable<Bid> GetBids()
    {
        return new[]
        {
            new Bid("winner123")
            {
                Id = 1,
                Amount = 40000.0,
                BidTime = DateTime.UtcNow.AddHours(-1),
                AuctionId = 1, // First auction ID
                VehicleId = 1  // Mazda's ID
            },
            new Bid("winner1235")
            {
                Id = 2,
                Amount = 30000.0,
                BidTime = DateTime.UtcNow.AddMinutes(-30),
                AuctionId = 1,
                VehicleId = 5  // Honda's ID
            }
        };
    }
}