using FluentValidation;

namespace sp_back_api.DTOs;

public record DeleteVehicleRequest
{
    public Guid Id { get; set; }
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