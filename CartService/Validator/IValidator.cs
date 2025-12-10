using Shared.Contracts;

namespace CartService.Validator
{
    public interface IValidator<T>
    {
        IValidator<T> SetNext(IValidator<T> i_NextValidator);
        ValidationResult Handle(T i_Object);
    }
}
