using CartService.Interfaces;
using CartService.Validator.Validators;
using CartService.Validator.Validators.OrderValidators;
using CartService.Validator.Validators.ItemValidators;
using CartService.Validator.Validators.OrderRequestsValidators;
using CartService.Data;
using Shared.Contracts;
using Shared.Contracts.Orders;

namespace CartService.Validator.Factories
{
    public class ValidationFactory : IValidationFactory
    {
        private readonly IOrderRepository _repository;

        public ValidationFactory(IOrderRepository i_Repository)
        {
            _repository = i_Repository;
        }

        public IValidator<CreateOrderRequest> GetRequestChain()
        {
            IValidator<CreateOrderRequest> validator1 = new OrderRequestIdRequiredValidator();
            IValidator<CreateOrderRequest> validator2 = new NumOfItemsValidator();
            IValidator<CreateOrderRequest> validator3 = new UniqueOrderIdRequestValidator(_repository);

            validator1.SetNext(validator2);
            validator2.SetNext(validator3);

            return validator1;
        }

        public IValidator<Order> GetOrderChain()
        {
            IValidator<Order> validator1 = new OrderIdValidator();
            IValidator<Order> validator2 = new CustomerIdValidator();
            IValidator<Order> validator3 = new TotalAmountValidator();
            IValidator<Order> validator4 = new UniqueItemIdValidator();
            IValidator<Order> validator5 = new CurrencyValidator();

            validator1.SetNext(validator2);
            validator2.SetNext(validator3);
            validator3.SetNext(validator4);
            validator4.SetNext(validator5);

            return validator1;
        }

        public IValidator<Item> GetItemChain()
        {
            IValidator<Item> validator1 = new QuantityValidator();
            IValidator<Item> validator2 = new PriceValidator();

            validator1.SetNext(validator2);

            return validator1;
        }
    }
}

