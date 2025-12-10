using CartService.Data;
using Shared.Contracts;

namespace CartService.Validator.Validators.OrderRequestsValidators
{
    public class UniqueOrderIdRequestValidator : BaseValidator<CreateOrderRequest>
    {
        private readonly IOrderRepository r_OrderRepository;

        public UniqueOrderIdRequestValidator(IOrderRepository i_OrderRepository)
        {
            r_OrderRepository = i_OrderRepository;
        }

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

            if (r_OrderRepository.Exists(i_Request.orderId))
            {
                return ValidationResult.Failure(
                    $"Order with ID {i_Request.orderId} already exists");
            }

            return base.Handle(i_Request);
        }
    }
}