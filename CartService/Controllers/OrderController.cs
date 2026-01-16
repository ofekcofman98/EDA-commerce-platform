using CartService.OrderCreation;
using CartService.OrderUpdate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Shared.Contracts;

namespace CartService.Controllers
{
    [ApiController]
    //[Route("create-order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderCreationService _orderCreationService;
        private readonly IOrderUpdateService _orderUpdateService;

        public OrderController(IOrderCreationService i_OrderCreationService, IOrderUpdateService i_OrderUpdateService)
        {
            _orderCreationService = i_OrderCreationService;
            _orderUpdateService = i_OrderUpdateService;
        }

        [HttpPost("create-order")]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest i_Request)
        {
            ServiceResponse result = _orderCreationService.CreateNewOrder(i_Request);

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
            var response = await _orderUpdateService.UpdateOrderStatus(i_Request);

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
