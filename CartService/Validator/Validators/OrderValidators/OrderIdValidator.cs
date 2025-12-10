using Shared.Contracts;

namespace CartService.Validator.Validators.OrderValidators
{
    public class OrderIdValidator : BaseValidator<Order>
    {
        public override ValidationResult Handle(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("No order found");
            }

            else if (string.IsNullOrWhiteSpace(i_Order.OrderId))
            {
                return ValidationResult.Failure("OrderId can't be empty");
            }

            return base.Handle(i_Order);
        }
    }
}
