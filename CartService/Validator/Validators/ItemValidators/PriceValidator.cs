using CartService.Interfaces;
using Shared.Contracts;

namespace CartService.Validator.Validators.ItemValidators
{
    public class PriceValidator : BaseValidator<Item>
    {
        public override ValidationResult Handle(Item i_Object)
        {
            if (i_Object == null)
            {
                return ValidationResult.Failure("No object found");
            }

            else if (i_Object.price < 0)
            {
                return ValidationResult.Failure("price must be non-negative");
            }

            return base.Handle(i_Object);
        }
    }
}
