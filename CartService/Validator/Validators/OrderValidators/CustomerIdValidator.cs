using CartService.Interfaces;
using Shared.Contracts.Orders;

namespace CartService.Validator.Validators.OrderValidators
{
    public class CustomerIdValidator : BaseValidator<Order>
    {
        public override ValidationResult Handle(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("No order found");
            }

            else if (i_Order.CustomerId == null)
            {
                return ValidationResult.Failure("CustomerID is null");
            }

            return base.Handle(i_Order);
        }


    }
}
