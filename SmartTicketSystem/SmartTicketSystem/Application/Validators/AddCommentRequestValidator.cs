using FluentValidation;

using SmartTicketSystem.Application.DTOs.AddTicketCommentDto;

namespace SmartTicketSystem.Application.Validators;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Comment message cannot be empty.")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");

        RuleFor(x => x.IsInternal)
            .NotNull().WithMessage("IsInternal flag must be provided.");
    }
}