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
    /// Endpoints for browsing the product catalog and, for administrators,
    /// updating, deleting, and syncing products from the external catalog source.
    /// </summary>
    /// <remarks>
    /// Response convention: every action returns HTTP 200 with an <see cref="APIResult{TData}"/>
    /// envelope. Success is indicated by <c>Status = true</c> and a populated <c>Data</c>;
    /// failures are also returned as HTTP 200 with <c>Status = false</c> and a populated
    /// <c>Error</c> (domain errors are not mapped to HTTP status codes). All actions require a
    /// valid bearer token; admin-only actions additionally require the <c>admin</c> claim.
    /// </remarks>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService;
        }

        /// <summary>Returns the full product catalog.</summary>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is the list of products.</returns>
        /// <response code="200">Envelope with the product list (<c>Status = true</c>) or an error.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<List<Product>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<APIResult<List<Product>>> GetProducts()
        {
            var result = await _productsService.GetProducts();

            if (result.Status)
            {
                return new APIResult<List<Product>>(result.Data);
            }
            else
            {
                return new APIResult<List<Product>>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>Updates an existing product (admin only). Only non-empty fields are applied.</summary>
        /// <param name="request">The product id plus the fields to update (title, price, description, rating).</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is the updated product on success.</returns>
        /// <response code="200">Envelope with the updated product (<c>Status = true</c>) or an error (e.g. product not found).</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        /// <response code="403">Authenticated but missing the required <c>admin</c> claim.</response>
        [RequiresClaim(Identity.AdminUserClaim, "true")]
        [HttpPut]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<APIResult<Product>> UpdateProduct([FromBody] UpdateProductRequest request)
        {
            if (request == null)
            {
                return new APIResult<Product>(ProjectErrorCodes.InvalidPayload, "Given identifier is null or empty");
            }

            var result = await _productsService.UpdateProduct(request);

            if (result.Status)
            {
                return new APIResult<Product>(result.Data, "Product has been updated successfully");
            }
            else
            {
                return new APIResult<Product>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>Deletes a product by id (admin only).</summary>
        /// <param name="identifier">The integer id of the product to delete.</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is a confirmation message on success.</returns>
        /// <response code="200">Envelope confirming deletion (<c>Status = true</c>) or an error (e.g. product not found).</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        /// <response code="403">Authenticated but missing the required <c>admin</c> claim.</response>
        [RequiresClaim(Identity.AdminUserClaim, "true")]
        [HttpDelete]
        [Route("{identifier}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<APIResult<string>> DeleteProduct(int identifier)
        {
            var result = await _productsService.DeleteProduct(identifier);

            if (result.Status)
            {
                return new APIResult<string>("Product has been deleted successfully");
            }
            else
            {
                return new APIResult<string>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>
        /// Triggers an on-demand catalog sync from the external source (admin only) and returns the
        /// current product list.
        /// </summary>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is the product list.</returns>
        /// <remarks>
        /// The sync is fired via the Quartz scheduler and the list is read immediately afterwards, so
        /// the returned data may not yet reflect the freshly synced catalog.
        /// </remarks>
        /// <response code="200">Envelope with the product list (<c>Status = true</c>) or an error.</response>
        /// <response code="401">Missing or invalid bearer token.</response>
        /// <response code="403">Authenticated but missing the required <c>admin</c> claim.</response>
        [RequiresClaim(Identity.AdminUserClaim, "true")]
        [HttpGet]
        [Route("sync")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<List<Product>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<APIResult<List<Product>>> SyncProducts()
        {
            var result = await _productsService.SyncProductsOnDemand();

            if (result.Status)
            {
                return new APIResult<List<Product>>(result.Data);
            }
            else
            {
                return new APIResult<List<Product>>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }
    }
}
