using Shared.Contracts;

namespace CartService.Validator.Validators
{
    public class NumOfItemsValidator : BaseValidator<Order>
    {
        public override ValidationResult Handle(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("No order found");
            }

            return base.Handle(i_Order);
        }

    }
}
