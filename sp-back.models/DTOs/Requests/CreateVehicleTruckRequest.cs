using FluentValidation;
namespace sp_back.models.DTOs.Requests;

public record CreateVehicleTruckRequest : CreateVehicleRequest
{
    public CreateVehicleTruckRequest(string manufacturer, string model, DateTime productionDate, double startingPrice, string vin, double loadCapacity) : base(manufacturer, model, productionDate, startingPrice, vin)
    {
        LoadCapacity = loadCapacity;
    }

    public double LoadCapacity { get; set;}
}

public class CreateVehicleTruckRequestValidator : AbstractValidator<CreateVehicleTruckRequest>
{
    public CreateVehicleTruckRequestValidator()
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
        
        RuleFor(x => x.LoadCapacity)
            .NotNull()
            .GreaterThan(0)
            .WithMessage("Trucks must have a valid load capacity");
    }
}