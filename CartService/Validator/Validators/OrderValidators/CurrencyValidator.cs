using Shared.Contracts;
using Shared.Contracts.Orders;

namespace CartService.Validator.Validators.OrderValidators
{
    public class CurrencyValidator : BaseValidator<Order>
    {
        public override ValidationResult Handle(Order i_Order)
        {
            if (string.IsNullOrWhiteSpace(i_Order.Currency))
            {
                return ValidationResult.Failure("Currency is required");
            }

            if (!Currencies.s_AllowedCurrencies.Contains(i_Order.Currency))
            {
                return ValidationResult.Failure($"Unsupported currency: {i_Order.Currency}");
            }

            return base.Handle(i_Order);
        }
    }

}

