using FluentValidation;
using sp_back.models.Enums;

namespace sp_back_api.DTOs;

public record UpdateVehicleRequest
{
    public Guid Id { get; set; }
    public string? Make { get; init; }
    public string? Model { get; init; }
    public DateTime? ProductionDate { get; init; }
    public uint? NumberOfDoors { get; init; }
    public uint? NumberOfSeats { get; init; }
    public double? LoadCapacity { get; init;}
    public VehicleType? Type { get; init; }
    public double? StartingPrice { get; init; }
    public string? VIN { get; init; }
}

public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleRequest>
{
    public UpdateVehicleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vehicle ID must be provided.");

        // Only validate the fields if they are provided
        RuleFor(x => x.VIN)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.VIN))
            .WithMessage("VIN must be provided if specified.");

        RuleFor(x => x.Make)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.Make))
            .WithMessage("Make cannot be empty.");

        RuleFor(x => x.Model)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.Model))
            .WithMessage("Model cannot be empty.");

        RuleFor(x => x.ProductionDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.ProductionDate.HasValue)
            .WithMessage("Production date must be a valid date and not in the future.");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0)
            .When(x => x.StartingPrice.HasValue)
            .WithMessage("Starting price must be greater than zero.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Vehicle type must be a valid enum value.");

        // Type-specific validation
        When(x => x.Type == VehicleType.Suv, () =>
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

        When(x => x.Type == VehicleType.Sedan, () =>
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

        When(x => x.Type == VehicleType.Truck, () =>
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