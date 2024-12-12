using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sp_back_api.DTOs;
using sp_back_api.DTOs.Responses;
using sp_back_api.Extensions;
using sp_back_api.Services;
using sp_back.models.DTOs.Requests;

namespace sp_back_api.Handlers;

public static class AuctionHandlers
{
    public static async Task<IResult> CreateAuction(
        [FromBody] CreateAuctionRequest request, 
        IAuctionService auctionService,
        IValidator<CreateAuctionRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var auction = await auctionService.CreateAuctionAsync(request);
        return Results.Created($"/auctions/{auction.Id}", AuctionResponseHandler.BuildResponse(auction));
    }

    public static async Task<IResult> GetActiveAuctions(IAuctionService auctionService)
    {
        var auctions = await auctionService.GetActiveAuctionsAsync();
        var response = new GetAllActiveAuctionsResponse
        {
            ActiveAuctions = auctions.Select(a => AuctionResponseHandler.BuildResponse(a)).ToList()
        };
        return Results.Ok(response);
    }

    public static async Task<IResult> GetAllAuctions(IAuctionService auctionService)
    {
        var auctions = await auctionService.GetAllAuctionsAsync();
        var response = new GetAllActiveAuctionsResponse
        {
            ActiveAuctions = auctions.Select(a => AuctionResponseHandler.BuildResponse(a)).ToList()
        };
        return Results.Ok(response);
    }

    public static async Task<IResult> PlaceBid(
        [FromBody] PlaceBidRequest request, 
        IAuctionService auctionService)
    {
        var auction = await auctionService.PlaceBidAsync(request);
        var bid = auction.GetBidInformation(request.BidderId, request.VehicleVin);
        var response = new PlaceBidResponse
        {
            BidId = bid.Id,
            Bidder = bid.BidderId,
            Amount = bid.Amount,
            BidTime = bid.BidTime,
            Vehicle = $"{bid.Vehicle.Make}{bid.Vehicle.Model}"
        };
        return Results.Ok(response);
    }

    public static async Task<IResult> AddVehiclesToAuction(
        [FromBody] AddVehiclesToAuctionRequest request, 
        IAuctionService auctionService)
    {
        var auction = await auctionService.AddVehiclesToAuctionAsync(request);
        return Results.Ok(AuctionResponseHandler.BuildResponse(auction));
    }

    public static async Task<IResult> RemoveVehiclesFromAuction(
        [FromBody] RemoveVehiclesFromAuctionRequest request,
        IAuctionService auctionService)
    {
        var auction = await auctionService.RemoveVehiclesFromAuctionAsync(request);
        return Results.Ok(AuctionResponseHandler.BuildResponse(auction));
    }

    public static async Task<IResult> CancelAuction(
        [FromBody] CancelAuctionResponse request, 
        IAuctionService auctionService)
    {
        var auction = await auctionService.CancelAuctionAsync(request.AuctionName);
        return Results.Ok(AuctionResponseHandler.BuildResponse(auction));
    }

    public static async Task<IResult> CloseAuction(
        [FromBody] CloseAuctionRequest request, 
        IAuctionService auctionService)
    {
        var auction = await auctionService.CloseAuctionAsync(request.AuctionName);
        return Results.Ok(AuctionResponseHandler.BuildCloseResponse(auction));
    }

    public static async Task<IResult> StartAuction(
        [FromBody] StartAuctionRequest request, 
        IAuctionService auctionService)
    {
        var auction = await auctionService.StartAuctionAsync(request.AuctionName);
        return Results.Ok(AuctionResponseHandler.BuildStartResponse(auction));
    }}