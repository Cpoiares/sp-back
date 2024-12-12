﻿using sp_back_api.DTOs.Responses;
using sp_back.models.Enums;
using sp_back.models.Exceptions;

namespace sp_back.models.Models.Vehicles;

public class Truck : Vehicle
{
    public double LoadCapacity { get; set; }
    public TruckInfo GetTruckInfo()
    {
        return new TruckInfo()
        {
            LoadCapacity = LoadCapacity,
            Make = Make,
            Model = Model,
            ProductionDate = ProductionDate.ToString()
        };
    }

    public override VehicleResponse GetVehicleResponses()
    {
        return new VehicleResponse()
        {
            Id = Id,
            Vin = VIN,
            Make = Make,
            Model = Model,
            LoadCapacity = LoadCapacity,
            Available = IsAvailable,
            VehicleType = VehicleType.Truck,
            Sold = IsSold,
            StartingPrice = StartingPrice,
            AuctionId = Auction?.Id ?? null,
            BidderId = IsSold ? Auction.GetHighestBidderForVehicle(Id) : null
        };    
    }
}

public class TruckInfo
{
    public double LoadCapacity { get; set; }
    public string Make { get; set;}
    public string Model { get; set;}
    public string ProductionDate { get; set;}
}