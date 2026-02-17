using Shared.Contracts;
using CartService.Validator;

namespace CartService.Interfaces
{
    public interface IValidator<T>
    {
        IValidator<T> SetNext(IValidator<T> i_NextValidator);
        ValidationResult Handle(T i_Object);
    }
}
