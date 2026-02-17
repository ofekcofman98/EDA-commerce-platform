using CartService.Interfaces;
using Shared.Contracts;

namespace CartService.Validator
{
    public class BaseValidator<T> : IValidator<T>
    {
        private IValidator<T>? _nextValidator;

        public IValidator<T> SetNext(IValidator<T> i_NextValidator)
        {
            _nextValidator = i_NextValidator;
            return i_NextValidator;
        }

        public virtual ValidationResult Handle(T i_Object)
        {
            if (_nextValidator != null)
            {
                return _nextValidator.Handle(i_Object);
            }

            return ValidationResult.Success();
        }

    }
}
