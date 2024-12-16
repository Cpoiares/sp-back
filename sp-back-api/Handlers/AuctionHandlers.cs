using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sp_back_api.Extensions;
using sp_back_api.Services;
using sp_back.models.DTOs.Requests;
using sp_back.models.DTOs.Responses;

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
            Auctions = auctions.Select(a => AuctionResponseHandler.BuildResponse(a)).ToList()
        };
        return Results.Ok(response);
    }

    public static async Task<IResult> GetAllAuctions(IAuctionService auctionService)
    {
        var auctions = await auctionService.GetAllAuctionsAsync();
        var response = new GetAllActiveAuctionsResponse
        {
            Auctions = auctions.Select(a => AuctionResponseHandler.BuildResponse(a)).ToList()
        };
        return Results.Ok(response);
    }

    public static async Task<IResult> PlaceBid(
        [FromBody] PlaceBidRequest request, 
        IValidator<PlaceBidRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var auction = await auctionService.PlaceBidAsync(request);
        var bid = auction.GetBidInformation(request.BidderId, request.VehicleVin);
        var response = new PlaceBidResponse
        {
            BidId = bid?.Id,
            Bidder = bid?.BidderId ?? throw new InvalidDataException("Bidder ID is invalid"),
            Amount = bid.Amount,
            BidTime = bid.BidTime,
            Vehicle = $"{bid.Vehicle?.Manufacturer}{bid.Vehicle?.Model}"
        };
        return Results.Ok(response);
    }

    public static async Task<IResult> AddVehiclesToAuction(
        [FromBody] AddVehiclesToAuctionRequest request, 
        IValidator<AddVehiclesToAuctionRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        var auction = await auctionService.AddVehiclesToAuctionAsync(request);
        return Results.Ok(AuctionResponseHandler.BuildResponse(auction));
    }

    public static async Task<IResult> RemoveVehiclesFromAuction(
        [FromBody] RemoveVehiclesFromAuctionRequest request,
        IValidator<RemoveVehiclesFromAuctionRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var auction = await auctionService.RemoveVehiclesFromAuctionAsync(request);
        return Results.Ok(AuctionResponseHandler.BuildResponse(auction));
    }

    public static async Task<IResult> CancelAuction(
        [FromBody] CloseAuctionRequest request, 
        IValidator<CloseAuctionRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var auction = await auctionService.CancelAuctionAsync(request.AuctionId);
        return Results.Ok(AuctionResponseHandler.BuildResponse(auction));
    }

    public static async Task<IResult> CloseAuction(
        [FromBody] CloseAuctionRequest request, 
        IValidator<CloseAuctionRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var auction = await auctionService.CloseAuctionAsync(request.AuctionId);
        return Results.Ok(AuctionResponseHandler.BuildCloseResponse(auction));
    }

    public static async Task<IResult> StartAuction(
        [FromBody] StartAuctionRequest request, 
        IValidator<StartAuctionRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var auction = await auctionService.StartAuctionAsync(request.AuctionId);
        return Results.Ok(AuctionResponseHandler.BuildStartResponse(auction));
    }

    public static async Task<IResult> CreateCollectiveAuction(
        [FromBody] CreateCollectiveAuctionRequest request, 
        IValidator<CreateCollectiveAuctionRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var auction = await auctionService.CreateCollectiveAuction(request);
        return Results.Ok(AuctionResponseHandler.BuildStartResponse(auction));
    }
    
    public static async Task<IResult> PlaceBidInCollectiveAuction(
        [FromBody] PlaceBidInCollectiveAuctionRequest request, 
        IValidator<PlaceBidInCollectiveAuctionRequest> validator,
        IAuctionService auctionService)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        var auction = await auctionService.PlaceBidInCollectiveAuction(request);
        return Results.Ok(AuctionResponseHandler.BuildStartResponse(auction));
    }
    
    public static async Task<IResult> GetAuctionBidHistory(
        int id, 
        IAuctionService auctionService)
    {
        var auction = await auctionService.GetAuctionAsync(id);
        return Results.Ok(AuctionResponseHandler.BuildBidHistoryResponse(auction));
    }

    public static async Task<IResult> GetCompletedAuctions(IAuctionService auctionService)
    {
        var auctions = await auctionService.GetCompletedAuctionsAsync();
        var response = new GetAllActiveAuctionsResponse
        {
            Auctions = auctions.Select(a => AuctionResponseHandler.BuildResponse(a)).ToList()
        };
        return Results.Ok(response);
    }
    
    
}