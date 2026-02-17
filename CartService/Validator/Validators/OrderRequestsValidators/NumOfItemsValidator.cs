using CartService.Interfaces;
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

            if (i_Request.numOfItems == null)
            {
                return ValidationResult.Failure("numOfItems is required");
            }

            if (i_Request.numOfItems <= 0)
            {
                return ValidationResult.Failure("numOfItems must be greater than 0");
            }

            return base.Handle(i_Request);
        }
    }
}