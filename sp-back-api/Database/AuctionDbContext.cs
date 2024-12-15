using Microsoft.EntityFrameworkCore;
using sp_back_api.Database.Data;
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
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            entity.HasMany(a => a.Vehicles)
                .WithOne(v => v.Auction)
                .HasForeignKey(v => v.AuctionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(a => a.Bids)
                .WithOne(b => b.Auction)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);
            

            entity.Property(e => e.StartTime);

            entity.Property(e => e.EndTime);
        });

        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.Amount)
                .HasColumnType("double");
            
            entity.HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.Manufacturer)
                .IsRequired();

            entity.Property(e => e.Model)
                .IsRequired();
            
            entity.HasOne(v => v.Auction)
                .WithMany(a => a.Vehicles)
                .HasForeignKey(v => v.AuctionId);
            
            entity.Property(e => e.StartingPrice)
                .IsRequired()
                .HasColumnType("double");
            
            entity.Property(e => e.IsDeleted)
                .IsRequired();
            
            entity.Property(e => e.IsAvailable)
                .IsRequired();
        });
        
        var vehicles = SeedData.GetVehicles().ToList();
        
        vehicles[0].AuctionId = 1; // Mazda Bid
        vehicles[4].AuctionId = 1; // Honda Bid
        
        modelBuilder.Entity<Suv>().Property(s => s.NumberOfSeats);
        modelBuilder.Entity<Sedan>().Property(s => s.NumberOfDoors);
        modelBuilder.Entity<Hatchback>().Property(h => h.NumberOfDoors);
        modelBuilder.Entity<Truck>().Property(t => t.LoadCapacity);
        
        modelBuilder.Entity<Hatchback>().HasData(vehicles.OfType<Hatchback>());
        modelBuilder.Entity<Sedan>().HasData(vehicles.OfType<Sedan>());
        modelBuilder.Entity<Suv>().HasData(vehicles.OfType<Suv>());
        modelBuilder.Entity<Truck>().HasData(vehicles.OfType<Truck>());
        
        var auctions = SeedData.GetAuctions().ToList();
        modelBuilder.Entity<Auction>().HasData(auctions);

        var bids = SeedData.GetBids().ToList();
        
        modelBuilder.Entity<Bid>().HasData(bids);
    }
}