using FluentValidation;

namespace EnglishTutor.Application.Features.ChatSessions.Commands.CreateSession
{
    public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
    {
        public CreateSessionCommandValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(120).WithMessage("Session title must not exceed 120 characters.")
                .When(x => !string.IsNullOrEmpty(x.Title));
        }
    }
}
