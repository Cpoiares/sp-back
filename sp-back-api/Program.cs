using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using sp_back_api.Database;
using sp_back_api.Database.Repository;
using sp_back_api.Database.Repository.Implementation;
using sp_back_api.Extensions;
using sp_back_api.Handlers;
using sp_back_api.Logging;
using sp_back_api.Middleware;
using sp_back_api.Services;
using sp_back_api.Services.Background.Implementation;
using sp_back_api.Services.Implementation;
using sp_back.models.Config;
using sp_back.models.DTOs.Responses;
using sp_back.models.Models.Vehicles;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);
var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../.."));

builder.Configuration
    .SetBasePath(projectRoot)
    .AddJsonFile(Path.Combine("conf", "appsettings.json"), optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseInMemoryDatabase("AuctionDb"));

builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
});

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddSingleton<IAuctionLogger, AuctionLogger>();
builder.Services.AddAuctionProcessingService();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vehicle Auction API",
        Version = "v1",
        Description = "An API for managing vehicle auctions",
        Contact = new OpenApiContact
        {
            Name = "Carlos Poiares",
            Email = "carlos.poiares2@gmail.com"
        }
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddValidators();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    context.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auction API");
    options.RoutePrefix = string.Empty;
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Vehicle endpoints
app.MapGet("/vehicles", VehicleHandlers.GetVehicles)
    .WithName("GetVehicles");
    
app.MapGet("/vehicles/{id:int}", VehicleHandlers.GetVehicleById)
    .WithName("GetVehicle")
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/vehicles/sedan", VehicleHandlers.CreateSedan)
    .WithName("CreateVehicle")
    .Produces<Vehicle>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPost("/vehicles/hatchback", VehicleHandlers.CreateHatchback)
    .WithName("CreateHatchback")
    .Produces<Vehicle>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPost("/vehicles/suv", VehicleHandlers.CreateSuv)
    .WithName("CreateSuv")
    .Produces<Vehicle>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPost("/vehicles/truck", VehicleHandlers.CreateTruck)
    .WithName("CreateTruck")
    .Produces<Vehicle>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPut("/vehicles/update", VehicleHandlers.UpdateVehicle)
    .WithName("UpdateVehicle")
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapDelete("/vehicles", VehicleHandlers.DeleteVehicle)
    .WithName("DeleteVehicle")
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

// Auction endpoints
app.MapPost("/auctions", AuctionHandlers.CreateAuction)
    .WithName("CreateAuction")
    .Produces<AuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest);

app.MapGet("/auctions/active", AuctionHandlers.GetActiveAuctions)
    .WithName("GetActiveAuctions")    
    .Produces<GetAllActiveAuctionsResponse>();

app.MapGet("/auctions/all", AuctionHandlers.GetAllAuctions)
    .WithName("GetAllAuctions")
    .Produces<GetAllActiveAuctionsResponse>();

app.MapGet("/auctions/completed", AuctionHandlers.GetCompletedAuctions)
    .WithName("GetCompletedAuctions")
    .Produces<GetAllActiveAuctionsResponse>();

app.MapPost("/auctions/bid", AuctionHandlers.PlaceBid)
    .WithName("PlaceBid")
    .Produces<PlaceBidResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/auctions/vehicles", AuctionHandlers.AddVehiclesToAuction)
    .WithName("AddVehiclesToAuction")
    .Produces<AuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapDelete("/auctions/vehicles", AuctionHandlers.RemoveVehiclesFromAuction)    
    .WithName("RemoveVehiclesFromAuction")
    .Produces<AuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/auctions/cancel", AuctionHandlers.CancelAuction)
    .WithName("CancelAuction")
    .Produces<AuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/auctions/close", AuctionHandlers.CloseAuction)
    .WithName("CloseAuction")
    .Produces<CloseAuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/auctions/start", AuctionHandlers.StartAuction)
    .WithName("StartAuction")
    .Produces<StartAuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapGet("/auctions/bid-history/{id:int}", AuctionHandlers.GetAuctionBidHistory)
    .WithName("GetAuctionBidHistory")
    .Produces<AuctionBidHistoryResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/auctions/create-collective", AuctionHandlers.CreateCollectiveAuction)
    .WithName("CreateCollectiveAuction")
    .Produces<AuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/auctions/collective-bid", AuctionHandlers.PlaceBidInCollectiveAuction)
    .WithName("PlaceCollectiveBid")
    .Produces<AuctionResponse>()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status404NotFound);

app.Run();

public partial class Program { }