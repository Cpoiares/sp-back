namespace sp_back.models.Config;

public class AppSettings
{
    public DatabaseSettings Database { get; set; }
    public LoggingSettings Logging { get; set; }
    public AuctionSettings Auction { get; set; }
}
