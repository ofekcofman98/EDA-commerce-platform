using Shared.Contracts;

namespace CartService.Validator.Validators.ItemValidators
{
    public class QuantityValidator : BaseValidator<Item>
    {
        public override ValidationResult Handle(Item i_Item)
        {
            if (i_Item == null)
            {
                return ValidationResult.Failure("No object found");
            }
            else if (i_Item.quantity <= 0)
            {
                return ValidationResult.Failure("Quantity must be greater than 0");
            }

            return base.Handle(i_Item);
        }

    }
}
