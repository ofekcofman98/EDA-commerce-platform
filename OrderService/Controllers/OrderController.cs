using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using Shared.Contracts.Orders;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("order-details")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet(Name = "order-details")]
        public IActionResult GetOrder(string orderId)
        {
            OrderDetails? orderDetails = _orderRepository.GetById(orderId);

            if (orderDetails == null)
            {
                return NotFound(new { error = "Order Not Found" });
            }

            return Ok(orderDetails);
        }

        [HttpGet(Name = "getAllOrderIdsFromTopic")]
        public IActionResult GetAllOrderIdsFromTopic(string topicName)
        {
            var orderIds = _orderRepository.GetAllOrderIdsFromTopic(topicName);

            if (orderIds == null)
            {
                return NotFound(new { error = "No Orders Found for the Given Topic" });
            }

            return Ok(orderIds);
        }

    }
}
