using FluentValidation;

namespace sp_back.models.DTOs.Requests;

public record DeleteVehicleRequest
{
    public DeleteVehicleRequest(int id)
    {
        Id = id;
    }

    public int Id { get; set;}
}

public class DeleteVehicleValidator : AbstractValidator<DeleteVehicleRequest>
{
    public DeleteVehicleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Vehicle Id is required");
    }
}