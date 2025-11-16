using CartService.Validator;
using CartService.Validator.Validators;
using Shared.Contracts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CartService.Generators;

namespace CartService
{
    public class OrderCreationService : IOrderCreationService
    {
        private readonly IOrderGenerator _orderGenerator;
        private readonly IItemGenerator _itemGenerator;

        private readonly IEventProducer _producer;


        public OrderCreationService(IOrderGenerator i_OrderGenerator, IItemGenerator i_ItemGenerator, IEventProducer producer)
        {
            _orderGenerator = i_OrderGenerator;
            _itemGenerator = i_ItemGenerator;
            _producer = producer;
        }

        public ServiceResponse CreateNewOrder(CreateOrderRequest i_Request)
        {
            ServiceResponse response = new ServiceResponse();

            Order newOrder = generateOrder(i_Request);

            ValidationResult orderResult = validateOrder(newOrder);
            if (!orderResult.isValid)
            {
                return new ServiceResponse { IsSuccesful = false, ErrorMessage = orderResult.ErrorMessage };
            }

            ValidationResult itemResult = validateItem(newOrder.Items);
            if (!itemResult.isValid)
            {
                return new ServiceResponse { IsSuccesful = false, ErrorMessage = itemResult.ErrorMessage };
            }

            _producer.PublishOrder(newOrder);

            return new ServiceResponse 
            { 
                IsSuccesful = true, 
                Order = newOrder,
                OrderId = newOrder.OrderId
            };
        }

        private Order generateOrder(CreateOrderRequest i_Request)
        {
            List<Item> items = _itemGenerator.GenerateItems(i_Request.numOfItems);
            Order newOrder = _orderGenerator.GenerateOrder(i_Request, items);
            
            return newOrder;
        }

        private ValidationResult validateOrder(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("Order is null");
            }

            IValidator<Order> chain = buildOrderValidationChain();
            ValidationResult result = chain.Handle(i_Order);

            return result;
        }

        private ValidationResult validateItem(IEnumerable<Item> i_Items)
        {
            IValidator<Item> validator = buildItemValidationChain();

            foreach (Item item in i_Items) 
            {
                ValidationResult result = validator.Handle(item);

                if (!result.isValid)
                {
                    return ValidationResult.Failure($"Validation failed for item {item.itemId}: {result.ErrorMessage}");
                }
            }

            return ValidationResult.Success();
        }

        private IValidator<Order> buildOrderValidationChain()
        {
            IValidator<Order> validator1 = new CustomerIdValidator();
            IValidator<Order> validator2 = new TotalAmountValidator();
            IValidator<Order> validator3 = new UniqueItemIdValidator();

            validator1.SetNext(validator2);
            validator2.SetNext(validator3);

            return validator1;
        }

        private IValidator<Item> buildItemValidationChain()
        {
            IValidator<Item> validator1 = new QuantityValidator();
            IValidator<Item> validator2 = new PriceValidator();

            validator1.SetNext(validator2);

            return validator1;
        }

    }
}
