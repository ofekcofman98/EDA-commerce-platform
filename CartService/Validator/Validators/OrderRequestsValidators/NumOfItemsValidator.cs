using Shared.Contracts;

namespace CartService.Validator.Validators.OrderRequestsValidators
{
    public class NumOfItemsValidator : BaseValidator<CreateOrderRequest>
    {
        public override ValidationResult Handle(CreateOrderRequest i_Request)
        {
            if (i_Request == null)
            {
                return ValidationResult.Failure("Request is null");
            }

            if (string.IsNullOrWhiteSpace(i_Request.orderId))
            {
                return ValidationResult.Failure("orderId is required");
            }

            return base.Handle(i_Request);
        }
    }
}