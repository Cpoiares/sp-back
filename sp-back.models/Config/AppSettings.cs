namespace sp_back.models.Config;

public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public AuctionSettings Auction { get; set; } = new();
}
