using FluentValidation;

namespace EnglishTutor.Application.Features.ChatMessages.Commands.SendMessage
{
    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .GreaterThan(0).WithMessage("Session ID must be greater than 0.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message content cannot be empty.")
                .MaximumLength(500).WithMessage("Question must not exceed 500 characters.");

            RuleFor(x => x.AnswerMode)
                .IsInEnum().WithMessage("Invalid answer mode selected.");
        }
    }
}
