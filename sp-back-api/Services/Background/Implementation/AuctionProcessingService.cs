using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sp_back.models.Config;

namespace sp_back_api.Services.Background.Implementation;

public class AuctionProcessingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionProcessingService> _logger;
    private readonly AppSettings _appSettings;

    public AuctionProcessingService(
        IServiceScopeFactory scopeFactory,
        ILogger<AuctionProcessingService> logger,
        IOptions<AppSettings> appSettings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _appSettings = appSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAuctions(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(
                    _appSettings.Auction.AuctionProcessingIntervalInSeconds), 
                    stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing auctions");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessAuctions(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var auctionService = scope.ServiceProvider.GetRequiredService<IAuctionService>();

        _logger.LogInformation("Starting auction processing cycle");

        try
        {
            await auctionService.ProcessCompletedAuctionsAsync();
            _logger.LogInformation("Completed auction processing cycle");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auction processing cycle");
            throw;
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Auction Processing Service is starting");
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Auction Processing Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}


public static class AuctionProcessingServiceExtensions
{
    public static IServiceCollection AddAuctionProcessingService(
        this IServiceCollection services)
    {
        services.AddHostedService<AuctionProcessingService>();
        return services;
    }
}