using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record CreateVehicleSedanHatchbackRequest : CreateVehicleRequest
{
    public CreateVehicleSedanHatchbackRequest(string manufacturer, string model, DateTime productionDate, double startingPrice, string vin, uint numberOfDoors) : base(manufacturer, model, productionDate, startingPrice, vin)
    {
        NumberOfDoors = numberOfDoors;
    }

    public uint NumberOfDoors { get; set;}
}

public class CreateVehicleSedanHatchbackValidator : AbstractValidator<CreateVehicleSedanHatchbackRequest>
{
    public CreateVehicleSedanHatchbackValidator()
    {
        RuleFor(x => x.Vin)
            .NotEmpty();
        
        RuleFor(x => x.Manufacturer)
            .NotEmpty();

        RuleFor(x => x.Model)
            .NotEmpty();

        RuleFor(x => x.ProductionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Production date must be a valid date and not in the future.");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0);
        
        RuleFor(x => x.NumberOfDoors)
            .NotNull();
    }
}