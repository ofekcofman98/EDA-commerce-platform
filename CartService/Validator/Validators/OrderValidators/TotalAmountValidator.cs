using CartService.Interfaces;
using Shared.Contracts.Orders;

namespace CartService.Validator.Validators.OrderValidators
{
    public class TotalAmountValidator : BaseValidator<Order>
    {
        public override ValidationResult Handle(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("No object found");
            }
            else if (i_Order.TotalAmount < 0)
            {
                return ValidationResult.Failure("Amount must be non-negative");
            }
            else if (i_Order.Items == null)
            {
                return ValidationResult.Failure("No items");
            }
            else if (i_Order.TotalAmount != getItemsSum(i_Order))
            {
                return ValidationResult.Failure("Total amount must equal the sum of item price * quantities");
            }

            return base.Handle(i_Order);
        }

        private decimal getItemsSum(Order i_Order)
        {
            return i_Order.Items.Sum(item => item.quantity * item.price);
        }

    }
}
