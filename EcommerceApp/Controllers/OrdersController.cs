using EcommerceApp.Constants;
using EcommerceApp.Filters;
using EcommerceApp.Models.Database;
using EcommerceApp.Models.Requests;
using EcommerceApp.Models.Responses.Base;
using EcommerceApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    /// <summary>
    /// Endpoints for placing orders, Stripe checkout and payment confirmation,
    /// dispatch, and invoice generation/retrieval. All actions require an authenticated user.
    /// </summary>
    /// <remarks>
    /// Response convention: most actions return HTTP 200 with an <see cref="APIResult{TData}"/>
    /// envelope. Success is indicated by <c>Status = true</c> and a populated <c>Data</c>;
    /// failures are also returned as HTTP 200 with <c>Status = false</c> and a populated
    /// <c>Error</c> (domain errors are not mapped to HTTP status codes). The one exception is
    /// <see cref="GetInvoice"/>, which streams a PDF file and returns HTTP 500 on failure.
    /// The <c>userId</c> parameters are bound from the <c>userid</c> JWT claim and are hidden
    /// from Swagger by <see cref="HideJwtUserIdParameterFilter"/>.
    /// </remarks>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        /// <summary>Creates a new order for the authenticated user from a list of product ids.</summary>
        /// <param name="userId">The owning user's id, bound from the JWT (hidden in Swagger).</param>
        /// <param name="request">The order payload containing the product ids to order.</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is the created <see cref="Order"/>.</returns>
        /// <response code="200">Envelope with the created order (<c>Status = true</c>) or an error (e.g. a product id does not exist).</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<Order>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<Order>> CreateOrder([FromJwtUserId] string userId, [FromBody] CreateOrderRequest request)
        {
            request.UserId = userId;
            var result = await _ordersService.CreateOrder(request);

            if (result.Status)
            {
                return new APIResult<Order>(result.Data, "Order has been created successfully");
            }
            else
            {
                return new APIResult<Order>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>Returns all orders in the system.</summary>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is the list of all orders.</returns>
        /// <response code="200">Envelope with the order list (<c>Status = true</c>) or an error.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<List<Order>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<List<Order>>> GetOrders()
        {
            var result = await _ordersService.GetOrders();

            if (result.Status)
            {
                return new APIResult<List<Order>>(result.Data);
            }
            else
            {
                return new APIResult<List<Order>>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>Returns the orders belonging to the authenticated user.</summary>
        /// <param name="userId">The owning user's id, bound from the JWT (hidden in Swagger).</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is the user's order list.</returns>
        /// <response code="200">Envelope with the user's orders (<c>Status = true</c>) or an error.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpGet]
        [Route("user-based")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<List<Order>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<List<Order>>> GetOrderByUserId([FromJwtUserId] string userId)
        {
            var result = await _ordersService.GetOrderByUserId(userId);

            if (result.Status)
            {
                return new APIResult<List<Order>>(result.Data);
            }
            else
            {
                return new APIResult<List<Order>>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>Creates a Stripe Checkout session for a submitted order and returns the payment URL.</summary>
        /// <param name="userId">The owning user's id, bound from the JWT (hidden in Swagger).</param>
        /// <param name="identifier">The order id to check out.</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is the Stripe Checkout redirect URL.</returns>
        /// <response code="200">Envelope with the redirect URL (<c>Status = true</c>) or an error (order missing or not in the Submitted state).</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpGet]
        [Route("checkout")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<string>> Checkout([FromJwtUserId] string userId, [FromQuery] string identifier)
        {
            var result = await _ordersService.Checkout(userId, identifier);

            if (result.Status)
            {
                return new APIResult<string>(result.Data, "Proceed to the URL to complete the payment");
            }
            else
            {
                return new APIResult<string>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>
        /// Confirms payment for an order: marks it Paid, generates the PDF invoice, emails it, and stores it.
        /// Invoked as the Stripe Checkout success redirect.
        /// </summary>
        /// <param name="userId">The owning user's id, bound from the JWT (hidden in Swagger).</param>
        /// <param name="identifier">The order id to confirm.</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is <c>true</c> when the order is confirmed.</returns>
        /// <response code="200">Envelope confirming payment (<c>Status = true</c>) or an error (missing/empty identifier, order not in the Submitted state, or already invoiced).</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpGet]
        [Route("checkout-confirm")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<bool>> ConfirmOrder([FromJwtUserId] string userId, [FromQuery] string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return new APIResult<bool>(ProjectErrorCodes.InvalidPayload, "Given identifier is null or empty");
            }

            var result = await _ordersService.ConfirmOrder(userId, identifier);

            if (result.Status)
            {
                return new APIResult<bool>(true, "Order has been paid !!");
            }
            else
            {
                return new APIResult<bool>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>Marks a paid, invoiced order as dispatched.</summary>
        /// <param name="userId">The owning user's id, bound from the JWT (hidden in Swagger).</param>
        /// <param name="identifier">The order id to dispatch.</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is <c>true</c> when the order is dispatched.</returns>
        /// <response code="200">Envelope confirming dispatch (<c>Status = true</c>) or an error (missing/empty identifier, or order not in the Paid+invoiced state).</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpPut]
        [Route("dispatch")]
        [ProducesResponseType(typeof(APIResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<bool>> DispatchOrder([FromJwtUserId] string userId, [FromQuery] string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return new APIResult<bool>(ProjectErrorCodes.InvalidPayload, "Given identifier is null or empty");
            }

            var result = await _ordersService.Dispatch(userId, identifier);

            if (result.Status)
            {
                return new APIResult<bool>(true, "Order has been dispatched !!");
            }
            else
            {
                return new APIResult<bool>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }

        }

        /// <summary>Downloads the stored PDF invoice for an order.</summary>
        /// <param name="userId">The owning user's id, bound from the JWT (hidden in Swagger).</param>
        /// <param name="identifier">The order id whose invoice should be downloaded.</param>
        /// <returns>The invoice PDF as a file stream, or HTTP 500 if no invoice is available.</returns>
        /// <remarks>
        /// Unlike the other actions, this endpoint does not use the <see cref="APIResult{TData}"/> envelope:
        /// it returns the raw PDF (<c>application/pdf</c>) on success and HTTP 500 on failure.
        /// </remarks>
        /// <response code="200">The invoice PDF file.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        /// <response code="500">No invoice exists for the order (or it could not be produced).</response>
        [HttpGet]
        [Route("invoice")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvoice([FromJwtUserId] string userId, [FromQuery] string identifier)
        {
            var result = await _ordersService.GetInvoice(userId, identifier);

            if (result.Status)
            {
                return File(result.Data, "application/pdf", $"Invoice_{identifier}.pdf");
            }

            return StatusCode(500, new { Error = "An internal server error occurred."});
        }

        /// <summary>Re-sends the stored invoice PDF for an order to the user's email.</summary>
        /// <param name="userId">The owning user's id, bound from the JWT (hidden in Swagger).</param>
        /// <param name="identifier">The order id whose invoice should be re-sent.</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is <c>true</c> when the email is sent.</returns>
        /// <response code="200">Envelope confirming the email was sent (<c>Status = true</c>) or an error (missing/empty identifier, or order with no stored invoice).</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpPost]
        [Route("resend-email")]
        [ProducesResponseType(typeof(APIResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<bool>> ResendEmail([FromJwtUserId] string userId, [FromQuery] string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return new APIResult<bool>(ProjectErrorCodes.InvalidPayload, "Given identifier is null or empty");
            }

            var result = await _ordersService.SendInvoice(userId, identifier);

            if (result.Status)
            {
                return new APIResult<bool>(true, "Email with the invoice pdf attached has been sent successfully");
            }
            else
            {
                return new APIResult<bool>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }
    }
}
