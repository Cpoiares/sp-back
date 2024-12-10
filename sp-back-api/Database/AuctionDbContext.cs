using Microsoft.EntityFrameworkCore;
using sp_back.models.Enums;
using sp_back.models.Models;
using sp_back.models.Models.Auction;
using sp_back.models.Models.Vehicles;

namespace sp_back_api.Database;

public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasMany<Vehicle>(a => a.Vehicles)
                .WithOne(v => v.Auction)
                .HasForeignKey(v => v.AuctionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(a => a.Bids)
                .WithOne(b => b.Auction)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Name)
                .IsRequired();

            entity.Property(e => e.StartTime)
                .IsRequired();

            entity.Property(e => e.EndTime)
                .IsRequired();
        });

        // Bid configuration
        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");
            
            entity.HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Make)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.HasOne(v => v.Auction)
                .WithMany(a => a.Vehicles)
                .HasForeignKey(v => v.AuctionId);
            
            entity.Property(e => e.StartingPrice)
                .IsRequired()
                .HasColumnType("double");
        });
        // Seed data
        var vehicles = new[]
        {
            new Vehicle
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
            new Vehicle
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
            new Vehicle
            {
                Id = Guid.Parse("3f00cb54-1534-4f23-b35f-4c3fb1e1c110"),
                Make = "Ford",
                Model = "Explorer",
                NumberOfDoors = 10,
                ProductionDate = DateTime.Today.AddMonths(-67),
                Type = VehicleType.Suv,
                StartingPrice = 35000,
                IsAvailable = true,
                VIN = "2"
            },
            new Vehicle
            {
                Id = Guid.Parse("d6ed42ce-e35b-4723-8174-83c26457c5d8"),
                Make = "Chevrolet",
                Model = "Silverado",
                NumberOfDoors = 10,
                ProductionDate = DateTime.Today.AddMonths(-37),
                Type = VehicleType.Truck,
                StartingPrice = 45000,
                IsAvailable = true,
                VIN = "3"
            },
            new Vehicle
            {
                Id = Guid.Parse("8b5e7a89-ef1d-4a07-9425-c884b0c148ab"),
                Make = "Honda",
                Model = "Civic",
                NumberOfDoors = 4,
                ProductionDate = DateTime.Today.AddMonths(-27),
                Type = VehicleType.Sedan,
                StartingPrice = 28000,
                IsAvailable = true,
                VIN = "4"
            },
            new Vehicle
            {
                Id = Guid.Parse("1a563e5f-7ce9-4c0f-a0f4-9e23a4e1b838"),
                Make = "Tesla",
                Model = "Model Y",
                NumberOfDoors = 4,
                ProductionDate = DateTime.Today.AddMonths(-17),
                Type = VehicleType.Suv,
                StartingPrice = 55000,
                IsAvailable = false,
                VIN = "5"
            }
        };
        
        vehicles[0].AuctionId = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"); // Mazda
        vehicles[4].AuctionId = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"); // Honda
        
        modelBuilder.Entity<Vehicle>().HasData(vehicles);

        var auctions = new[]
        {
            new Auction
            {
                Id = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"),
                Name = "Premium Auction",
                StartTime = DateTime.UtcNow.AddDays(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                Status = AuctionStatus.Active,
            },
            new Auction
            {
                Id = Guid.Parse("1e43c7e7-2e7e-4c5a-9a6d-8c9b25c5c556"),
                Name = "Luxury Special",
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddDays(5),
                Status = AuctionStatus.Active,
            },
            new Auction
            {
                Id = Guid.Parse("9d2e6f0e-b5a2-4c8f-b3e4-3f2e6a3b123c"),
                Name = "Upcoming Auction",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(6),
                Status = AuctionStatus.Active,

            }
        };
        modelBuilder.Entity<Auction>().HasData(auctions);

        var bids = new[]
        {
            new Bid
            {
                Id = Guid.NewGuid(),
                BidderId = "winner123",
                Amount = 40000.0,
                BidTime = DateTime.UtcNow.AddHours(-1),
                AuctionId = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"), // First auction ID
                VehicleId = Guid.Parse("c5ae495d-9bd7-4256-981c-931d2b3bd947")  // Mazda's ID
            },
            new Bid
            {
                Id = Guid.NewGuid(),
                BidderId = "winner123",
                Amount = 30000.0,
                BidTime = DateTime.UtcNow.AddMinutes(-30),
                AuctionId = Guid.Parse("2d73eb5c-c7b5-4a6a-9a3f-616b7d456ed1"),
                VehicleId = Guid.Parse("8b5e7a89-ef1d-4a07-9425-c884b0c148ab")  // Honda's ID
            }
        };


        modelBuilder.Entity<Bid>().HasData(bids);
    }
}