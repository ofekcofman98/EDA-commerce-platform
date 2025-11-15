using Shared.Contracts;

namespace CartService.Validator.Validators
{
    public class CustomerIdValidator : BaseValidator<Order>
    {
        public override ValidationResult Handle(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("No object found");
            }

            else if (i_Order.CustomerId == null)
            {
                return ValidationResult.Failure("CustomerID is null");
            }

            return base.Handle(i_Order);
        }


    }
}
