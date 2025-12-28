using FluentValidation;

using SmartTicketSystem.Application.DTOs.Auth;
public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(3).WithMessage("Full name must be at least 3 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches("[A-Z]").WithMessage("Must contain at least 1 uppercase letter")
            .Matches("[0-9]").WithMessage("Must contain at least 1 number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain at least 1 special character");

        RuleFor(x => x.RoleIds)
            .NotEmpty().WithMessage("At least one role must be assigned")
            .Must(r => r.All(id => id > 0)).WithMessage("Invalid role id detected");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[6-9]\d{9}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MinimumLength(5).WithMessage("Address is too short");
    }
}