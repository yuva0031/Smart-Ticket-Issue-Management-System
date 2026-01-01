using FluentValidation;

using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Validators;

public class CreateTicketHistoryValidator : AbstractValidator<CreateTicketHistoryDto>
{
    public CreateTicketHistoryValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.FieldName).NotEmpty().WithMessage("Field name is required");
        RuleFor(x => x.NewValue).NotEmpty().WithMessage("New value cannot be empty");
    }
}