using CartService.OrderCreation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Shared.Contracts;

namespace CartService.Controllers
{
    [ApiController]
    [Route("create-order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderCreationService _orderCreationService;

        public OrderController(IOrderCreationService i_OrderCreationService)
        {
            _orderCreationService = i_OrderCreationService;
        }

        [HttpPost(Name = "create-order")]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest i_Request)
        {
            ServiceResponse result = _orderCreationService.CreateNewOrder(i_Request);

            if (result.IsSuccesful)
            {
                return Created($"api/orders/{result.OrderId}", result.Order);
            }
            else
            {
                return BadRequest(new { error = "Validation Failed", detail = result.ErrorMessage });
            }
        }

    }
}
