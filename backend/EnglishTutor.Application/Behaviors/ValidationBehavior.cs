using FluentValidation;
using MediatR;

namespace EnglishTutor.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            foreach (var validator in _validators)
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);
            }

            return await next(cancellationToken);
        }
    }
}
