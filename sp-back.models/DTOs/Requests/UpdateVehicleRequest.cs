using FluentValidation;
using sp_back.models.Enums;

namespace sp_back.models.DTOs.Requests;

public record UpdateVehicleRequest
{
    public UpdateVehicleRequest() { }
    public UpdateVehicleRequest(string? manufacturer, string? model, DateTime? productionDate, uint? numberOfDoors, uint? numberOfSeats, double? loadCapacity, VehicleType? type, double? startingPrice, int id)
    {
        Manufacturer = manufacturer;
        Model = model;
        ProductionDate = productionDate;
        NumberOfDoors = numberOfDoors;
        NumberOfSeats = numberOfSeats;
        LoadCapacity = loadCapacity;
        Type = type;
        StartingPrice = startingPrice;
        Id = id;
    }

    public int Id { get; set; }
    public string? Manufacturer { get; set;}
    public string? Model { get; set;}
    public DateTime? ProductionDate { get; set;}
    public uint? NumberOfDoors { get; set;}
    public uint? NumberOfSeats { get; set;}
    public double? LoadCapacity { get; set;}
    public VehicleType? Type { get; set;}
    public double? StartingPrice { get; set;}
}

public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vehicle ID must be provided.");
        
        When(x => x.StartingPrice.HasValue, () =>
        {
            RuleFor(x => x.StartingPrice)
                .GreaterThan(0)
                .WithMessage("Starting price must be greater than 0");
        });
        
        When(x => x.Type.HasValue && x.Type == VehicleType.Suv, () =>
        {
            RuleFor(x => x.NumberOfSeats)
                .NotNull()
                .When(x => x.NumberOfSeats.HasValue)
                .WithMessage("Number of seats must be provided for SUVs.");

            RuleFor(x => x.LoadCapacity)
                .Must(value => value == null || value == 0)
                .When(x => x.LoadCapacity.HasValue)
                .WithMessage("Load capacity is not applicable for SUVs.");

            RuleFor(x => x.NumberOfDoors)
                .Must(value => value == null || value == 0)
                .When(x => x.NumberOfDoors.HasValue)
                .WithMessage("Number of doors is not applicable for SUVs.");
        });

        When(x => x.Type.HasValue && x.Type == VehicleType.Sedan, () =>
        {
            RuleFor(x => x.NumberOfDoors)
                .NotNull()
                .When(x => x.NumberOfDoors.HasValue)
                .WithMessage("Number of doors must be provided for sedans.");

            RuleFor(x => x.LoadCapacity)
                .Must(value => value == null || value == 0)
                .When(x => x.LoadCapacity.HasValue)
                .WithMessage("Load capacity is not applicable for sedans.");

            RuleFor(x => x.NumberOfSeats)
                .Must(value => value == null || value == 0)
                .When(x => x.NumberOfSeats.HasValue)
                .WithMessage("Number of seats is not applicable for sedans.");
        });

        When(x => x.Type.HasValue && x.Type == VehicleType.Truck, () =>
        {
            RuleFor(x => x.LoadCapacity)
                .NotNull()
                .GreaterThan(0)
                .When(x => x.LoadCapacity.HasValue)
                .WithMessage("Trucks must have a valid load capacity.");

            RuleFor(x => x.NumberOfDoors)
                .Must(value => value == null || value == 0)
                .When(x => x.NumberOfDoors.HasValue)
                .WithMessage("Number of doors is not applicable for trucks.");

            RuleFor(x => x.NumberOfSeats)
                .Must(value => value == null || value == 0)
                .When(x => x.NumberOfSeats.HasValue)
                .WithMessage("Number of seats is not applicable for trucks.");
        });
    }
}