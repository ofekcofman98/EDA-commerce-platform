using CartService.Data;
using CartService.Producer;
using Shared.Contracts;
using Shared.Contracts.Events;
using System.Text.Json;

namespace CartService.OrderUpdate
{
    public class OrderUpdateService : IOrderUpdateService
    {
        private readonly IOrderRepository _repository;
        private readonly IEventProducer _producer;

        public OrderUpdateService(IOrderRepository i_Repository, IEventProducer i_Producer)
        {
            _repository = i_Repository;
            _producer = i_Producer;
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

            existingOrder.Status = i_Request.OrderStatus;

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
    }
}
