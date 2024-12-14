namespace sp_back.models.DTOs.Requests;

public record CreateVehicleRequest
{
    protected CreateVehicleRequest(string make, string model, DateTime productionDate, double startingPrice, string vin)
    {
        Make = make;
        Model = model;
        ProductionDate = productionDate;
        StartingPrice = startingPrice;
        Vin = vin;
    }

    public string Make { get; init; }
    public string Model { get; init; }
    public DateTime ProductionDate { get; init; }
    public double StartingPrice { get; init; }
    public string Vin { get; init; }
}

// public class CreateVehicleValidator : AbstractValidator<CreateVehicleRequest>
// {
//     public CreateVehicleValidator()
//     {
//         RuleFor(x => x.Vin)
//             .NotEmpty();
//         
//         RuleFor(x => x.Make)
//             .NotEmpty();
//
//         RuleFor(x => x.Model)
//             .NotEmpty();
//
//         RuleFor(x => x.ProductionDate)
//             .NotEmpty()
//             .LessThanOrEqualTo(DateTime.UtcNow)
//             .WithMessage("Production date must be a valid date and not in the future.");
//
//         RuleFor(x => x.StartingPrice)
//             .GreaterThan(0);
//
//         RuleFor(x => x.Type)
//             .IsInEnum();
//
//         // Type-specific validation
//         When(x => x.Type == VehicleType.Suv, () =>
//         {
//             RuleFor(x => x.NumberOfSeats)
//                 .NotNull();
//
//             RuleFor(x => x.LoadCapacity)
//                 .Must(value => value == null || value == 0)
//                 .WithMessage("Load capacity is not applicable for SUVs");
//
//             RuleFor(x => x.NumberOfDoors)
//                 .Must(value => value == null || value == 0)
//                 .WithMessage("Number of doors is not applicable for SUVs");
//         });
//
//         When(x => x.Type == VehicleType.Sedan, () =>
//         {
//             RuleFor(x => x.NumberOfDoors)
//                 .NotNull();
//
//             RuleFor(x => x.LoadCapacity)
//                 .Must(value => value == null || value == 0)
//                 .WithMessage("Load capacity is not applicable for sedans");
//
//             RuleFor(x => x.NumberOfSeats)
//                 .Must(value => value == null || value == 0)
//                 .WithMessage("Number of seats is not applicable for sedans");
//         });
//
//         When(x => x.Type == VehicleType.Truck, () =>
//         {
//             RuleFor(x => x.LoadCapacity)
//                 .NotNull()
//                 .GreaterThan(0)
//                 .WithMessage("Trucks must have a valid load capacity");
//
//             RuleFor(x => x.NumberOfDoors)
//                 .Must(value => value == null || value == 0)
//                 .WithMessage("Number of doors is not applicable for trucks");
//
//             RuleFor(x => x.NumberOfSeats)
//                 .Must(value => value == null || value == 0)
//                 .WithMessage("Number of seats is not applicable for trucks");
//         });
//     }
// }
