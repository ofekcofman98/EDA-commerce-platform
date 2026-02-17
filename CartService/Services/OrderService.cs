using CartService.Interfaces;
using CartService.Validator;
using CartService.Data;
using Shared.Contracts;
using CartService.Generators;
using CartService.Producer;
using Shared.Contracts.Events;
using Shared.Contracts.Orders;
using System.Text.Json;

namespace CartService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderGenerator _orderGenerator;
        private readonly IItemGenerator _itemGenerator;
        private readonly IOrderRepository _repository;
        private readonly IEventProducer _producer;
        private readonly IValidationFactory _validationFactory;

        public OrderService(
            IOrderGenerator i_OrderGenerator, 
            IItemGenerator i_ItemGenerator, 
            IOrderRepository i_Repository, 
            IEventProducer i_Producer,
            IValidationFactory i_ValidationFactory)
        {
            _orderGenerator = i_OrderGenerator;
            _itemGenerator = i_ItemGenerator;
            _repository = i_Repository;
            _producer = i_Producer;
            _validationFactory = i_ValidationFactory;
        }

        public async Task<ServiceResponse> CreateNewOrder(CreateOrderRequest i_Request)
        {
            ValidationResult requestResult = validateRequest(i_Request);
            if(!requestResult.isValid)
            {
                return new ServiceResponse { IsSuccesful = false, ErrorMessage = requestResult.ErrorMessage};
            }

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

            _repository.Add(newOrder);

            var envelope = new EventEnvelope
            {
                EventType = EventType.OrderCreated,
                OrderId = newOrder.OrderId,
                Payload = JsonSerializer.SerializeToElement(newOrder)
            };

            await _producer.PublishAsync(envelope);

            return new ServiceResponse
            {
                IsSuccesful = true,
                Order = newOrder,
                OrderId = newOrder.OrderId
            };
        }

        public async Task<ServiceResponse> UpdateOrderStatus(UpdateOrderRequest i_Request)
        {
            if (i_Request == null)
            {
                return new ServiceResponse
                {
                    IsSuccesful = false,
                    ErrorMessage = "Invalid request"
                };
            }

            var existingOrder = _repository.GetById(i_Request.OrderId);

            if(existingOrder == null)
            {
                return new ServiceResponse
                {
                    IsSuccesful = false,
                    ErrorMessage = $"Order {i_Request.OrderId} not found"
                };
            }

            if(!Enum.TryParse<OrderStatus>(i_Request.Status, ignoreCase: true, out var status))
            {
                return new ServiceResponse
                {
                    IsSuccesful = false,
                    ErrorMessage = $"Invalid order status: {i_Request.Status}"
                };
            }

            existingOrder.Status = status;

            var envelope = new EventEnvelope
            {
                EventType = EventType.OrderUpdated,
                OrderId = existingOrder.OrderId,
                Payload = JsonSerializer.SerializeToElement(i_Request)
            };

            await _producer.PublishAsync(envelope);

            return new ServiceResponse
            {
                IsSuccesful = true,
                OrderId = existingOrder.OrderId,
                Order = existingOrder
            };
        }

        private Order generateOrder(CreateOrderRequest i_Request)
        {
            List<Item> items = _itemGenerator.GenerateItems(i_Request.numOfItems ?? 0);
            Order newOrder = _orderGenerator.GenerateOrder(i_Request, items);

            return newOrder;
        }

        private ValidationResult validateRequest(CreateOrderRequest i_Request)
        {
            if (i_Request == null)
            {
                return ValidationResult.Failure("Request is null");
            }

            IValidator<CreateOrderRequest> chain = _validationFactory.GetRequestChain();
            ValidationResult result = chain.Handle(i_Request);

            return result;
        }

        private ValidationResult validateOrder(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("Order is null");
            }

            IValidator<Order> chain = _validationFactory.GetOrderChain();
            ValidationResult result = chain.Handle(i_Order);

            return result;
        }

        private ValidationResult validateItem(IEnumerable<Item> i_Items)
        {
            IValidator<Item> validator = _validationFactory.GetItemChain();

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
    }
}

