using FluentValidation;

using SmartTicketSystem.Application.DTOs;
using SmartTicketSystem.Domain.Enums;

namespace SmartTicketSystem.Application.Validators;

public class CreateTicketDtoValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ticket title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters.");

        RuleFor(x => x.CategoryId)
            .InclusiveBetween(1, 7).When(x => x.CategoryId.HasValue)
            .WithMessage("Category must be between 1 and 7 if provided");

        RuleFor(x => x.PriorityId)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(value => Enum.IsDefined(typeof(TicketPriorityEnum), value))
            .WithMessage("Invalid priority selected.");
    }
}