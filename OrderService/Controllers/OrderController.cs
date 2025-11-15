using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using Shared.Contracts;

namespace OrderService.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet(Name = "order-details")]
        public IActionResult GetOrder(string i_OrderId)
        {
            OrderDetails? orderDetails = _orderRepository.GetById(i_OrderId);

            if (orderDetails == null)
            {
                return BadRequest(new { error = "Order Not Found" });
            }

            else
            {
                return Ok(orderDetails);
            }
        }
    }
}
