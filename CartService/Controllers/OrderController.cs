using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Shared.Contracts;

namespace CartService.Controllers
{
    [Route("api/orders")]
    [ApiController]
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
                return CreatedAtAction(
                    actionName: "GetOrderDetials",
                    routeValues: new { orderId = result.OrderId},
                    value: result.Order
                    );
            }
            else
            {
                return BadRequest(new { error = "Validation Failed", detail = result.ErrorMessage });
            }
        }

    }
}
