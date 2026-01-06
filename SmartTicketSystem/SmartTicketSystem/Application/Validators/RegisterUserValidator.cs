using FluentValidation;

using SmartTicketSystem.Application.DTOs.Auth;
using SmartTicketSystem.Domain.Enums;
public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
{
    private readonly int[] agentAssignableRoles = {
        (int)UserRoleEnum.SupportAgent
    };

    public RegisterUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(3).WithMessage("First name must be at least 3 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(3).WithMessage("Last name must be at least 3 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches("[A-Z]").WithMessage("Must contain at least 1 uppercase letter")
            .Matches("[0-9]").WithMessage("Must contain at least 1 number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain at least 1 special character");

        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("A role must be assigned")
            .Must(r => r > 0).WithMessage("Invalid role id detected");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[6-9]\d{9}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MinimumLength(5).WithMessage("Address is too short");

        When(x => agentAssignableRoles.Contains(x.RoleId), () =>
        {
            RuleFor(x => x.CategorySkillIds)
                .NotNull().WithMessage("Agent must have skill categories assigned")
                .NotEmpty().WithMessage("Agent must have at least one skill category")
                .Must(list => list.All(id => id > 0))
                .WithMessage("Skill category IDs must be valid positive numbers");
        });

    }
}