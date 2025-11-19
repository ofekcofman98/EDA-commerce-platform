using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using Shared.Contracts;

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
    }
}
