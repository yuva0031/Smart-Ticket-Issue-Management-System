using FluentValidation;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Domain.Enums;

namespace SmartTicketSystem.Application.Validators;

public class UpdateTicketDtoValidator : AbstractValidator<UpdateTicketDto>
{
    public UpdateTicketDtoValidator()
    {
        RuleFor(x => x.StatusId)
            .NotEmpty().WithMessage("Status update is required.")
            .Must(value => Enum.IsDefined(typeof(TicketStatusEnum), value))
            .WithMessage("Invalid status provided.");

        RuleFor(x => x.AssignedToId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("Assigned user id cannot be empty if provided.");

        RuleFor(x => x.PriorityId)
            .Must(value => value == null || Enum.IsDefined(typeof(TicketPriorityEnum), value))
            .WithMessage("Invalid priority selected.");

        RuleFor(x => x.DueDate)
            .Must(date => date == null || date > DateTime.UtcNow)
            .WithMessage("Due date must be a future date.");
    }
}