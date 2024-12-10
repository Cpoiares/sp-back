using FluentValidation;
using sp_back.models.Enums;

namespace sp_back_api.DTOs;

public record CreateVehicleRequest
{
    public string Make { get; init; }
    public string Model { get; init; }
    public DateTime ProductionDate { get; init; }
    public double? LoadCapacity { get; set; }
    public uint? NumberOfSeats { get; set; }
    public uint? NumberOfDoors { get; set;}
    public VehicleType Type { get; init; }
    public double StartingPrice { get; init; }
    public string VIN { get; init; }
}

public class CreateVehicleValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.VIN)
            .NotEmpty();
        
        RuleFor(x => x.Make)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Model)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ProductionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow);

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0);

        RuleFor(x => x.Type)
            .IsInEnum();

        // Type-specific validation
        When(x => x.Type == VehicleType.Suv, () =>
        {
            RuleFor(x => x.NumberOfSeats)
                .NotNull();

            RuleFor(x => x.LoadCapacity)
                .Must(value => value == null || value == 0)
                .WithMessage("Load capacity is not applicable for SUVs");

            RuleFor(x => x.NumberOfDoors)
                .Must(value => value == null || value == 0)
                .WithMessage("Number of doors is not applicable for SUVs");
        });

        When(x => x.Type == VehicleType.Sedan, () =>
        {
            RuleFor(x => x.NumberOfDoors)
                .NotNull();

            RuleFor(x => x.LoadCapacity)
                .Must(value => value == null || value == 0)
                .WithMessage("Load capacity is not applicable for sedans");

            RuleFor(x => x.NumberOfSeats)
                .Must(value => value == null || value == 0)
                .WithMessage("Number of seats is not applicable for sedans");
        });

        When(x => x.Type == VehicleType.Truck, () =>
        {
            RuleFor(x => x.LoadCapacity)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Trucks must have a valid load capacity");

            RuleFor(x => x.NumberOfDoors)
                .Must(value => value == null || value == 0)
                .WithMessage("Number of doors is not applicable for trucks");

            RuleFor(x => x.NumberOfSeats)
                .Must(value => value == null || value == 0)
                .WithMessage("Number of seats is not applicable for trucks");
        });
    }
}
