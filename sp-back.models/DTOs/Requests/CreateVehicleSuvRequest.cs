using FluentValidation;
namespace sp_back.models.DTOs.Requests;

public record CreateVehicleSuvRequest : CreateVehicleRequest
{
    
    public CreateVehicleSuvRequest(string make, string model, DateTime productionDate, double startingPrice, string vin, uint numberOfSeats) : base(make, model, productionDate, startingPrice, vin)
    {
        NumberOfSeats = numberOfSeats;
    }

    public uint NumberOfSeats { get; set; }
}

public class CreateVehicleSuvRequestValidator : AbstractValidator<CreateVehicleSuvRequest>
{
    public CreateVehicleSuvRequestValidator()
    {
        RuleFor(x => x.Vin)
            .NotEmpty();
        
        RuleFor(x => x.Make)
            .NotEmpty();

        RuleFor(x => x.Model)
            .NotEmpty();

        RuleFor(x => x.ProductionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Production date must be a valid date and not in the future.");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0);
        
        RuleFor(x => x.NumberOfSeats)
            .NotNull();
    }
}