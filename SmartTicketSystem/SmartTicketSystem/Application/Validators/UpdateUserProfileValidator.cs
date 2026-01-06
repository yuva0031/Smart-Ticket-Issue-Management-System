using FluentValidation;

using SmartTicketSystem.Application.DTOs;

namespace SmartTicketSystem.Application.Validators.Users;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^[6-9]\d{9}$")
            .WithMessage("Invalid phone number");

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(250);
    }
}