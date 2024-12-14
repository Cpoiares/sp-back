using System.Text.Json;
using Microsoft.Extensions.Options;
using sp_back_api.Loging;
using sp_back.models.Config;
using sp_back.models.Models.Auction;

public class AuctionLogger : IAuctionLogger
{
    private readonly string _logPath;
    private readonly object _lock = new object();

    public AuctionLogger(IOptions<AppSettings> appSettings)
    {
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));
        _logPath = Path.Combine(projectRoot, appSettings.Value.Logging.AuctionLogPath);
        var logDirectory = Path.GetDirectoryName(_logPath);
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }    
    }

    public async Task LogAuctionCompleted(Auction auction)
    {
        var soldVehicles = auction.Vehicles
            .Where(v => auction.GetHighestBidderForVehicle(v.Id) != null)
            .Select(vehicle => new
            {
                VehicleInfo = new
                {
                    vehicle.Id,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.Year,
                    vehicle.Type,
                    vehicle.StartingPrice
                },
                WinningBid = string.IsNullOrEmpty(auction.GetHighestBidderForVehicle(vehicle.Id)) ? null : auction.GetHighestBidForVehicle(vehicle.Id)?.Amount,
                WinningBidder = auction.GetHighestBidderForVehicle(vehicle.Id)
            })
            .ToList();

        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            AuctionId = auction.Id,
            AuctionName = auction.Name,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            TotalVehicles = auction.Vehicles.Count,
            SoldVehicles = soldVehicles.Count,
            SoldVehicleDetails = soldVehicles
        };

        var logLine = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        lock (_lock)
        {
            File.AppendAllText(_logPath, logLine + Environment.NewLine);
        }
    }
}