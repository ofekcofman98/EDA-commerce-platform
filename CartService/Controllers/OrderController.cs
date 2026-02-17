using CartService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Shared.Contracts;

namespace CartService.Controllers
{
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService i_OrderService)
        {
            _orderService = i_OrderService;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest i_Request)
        {
            ServiceResponse result = await _orderService.CreateNewOrder(i_Request);

            if (result.IsSuccesful)
            {
                return Created($"/create-order/{result.OrderId}", result.Order);
            }
            else
            {
                return BadRequest(new { error = "Validation Failed", detail = result.ErrorMessage });
            }
        }

        [HttpPut("update-order")]
        public async Task<IActionResult> UpdateOrder([FromBody] UpdateOrderRequest i_Request)
        {
            var response = await _orderService.UpdateOrderStatus(i_Request);

            if (response.IsSuccesful)
            {
                return Ok(response.Order);
            }
            else
            {
                return BadRequest(new { error = "Update Failed", detail = response.ErrorMessage });
            }
        }

    }
}
