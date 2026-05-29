using EcommerceApp.Constants;
using EcommerceApp.Models.Requests;
using EcommerceApp.Models.Responses;
using EcommerceApp.Models.Responses.Base;
using EcommerceApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApp.Controllers
{
    /// <summary>
    /// Endpoints for user registration and login (JWT issuance). Both actions are anonymous.
    /// </summary>
    /// <remarks>
    /// Response convention: every action returns HTTP 200 with an <see cref="APIResult{TData}"/>
    /// envelope. Success is indicated by <c>Status = true</c> and a populated <c>Data</c>;
    /// failures are also returned as HTTP 200 with <c>Status = false</c> and a populated
    /// <c>Error</c> (this project does not map domain errors to HTTP status codes).
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Registers a new user account.</summary>
        /// <param name="request">First name, last name, email, and password for the new account.</param>
        /// <returns>An <see cref="APIResult{TData}"/> whose <c>Data</c> is a confirmation message on success.</returns>
        /// <response code="200">
        /// Always returned. On success <c>Status = true</c>; on failure (e.g. email already exists,
        /// invalid payload) <c>Status = false</c> with <c>Error</c> populated.
        /// </response>
        [HttpPost]
        [Route("register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<string>), StatusCodes.Status200OK)]
        public async Task<APIResult<string>> Register([FromBody] UserRegisterRequest request)
        {
            var result = await _userService.Register(request);

            if (result.Status)
            {
                return new APIResult<string>("User has been registered successfully");
            }
            else
            {
                return new APIResult<string>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }

        /// <summary>Authenticates a user and issues a JWT bearer token.</summary>
        /// <param name="request">Email and password credentials.</param>
        /// <returns>
        /// An <see cref="APIResult{TData}"/> whose <c>Data</c> is a <see cref="UserLoginResponse"/>
        /// containing the signed JWT on success.
        /// </returns>
        /// <response code="200">
        /// Always returned. On success <c>Status = true</c> with the token; on failure
        /// (user not found, invalid credentials) <c>Status = false</c> with <c>Error</c> populated.
        /// </response>
        [HttpPost]
        [Route("login")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(APIResult<UserLoginResponse>), StatusCodes.Status200OK)]
        public async Task<APIResult<UserLoginResponse>> Login([FromBody] UserLoginRequest request)
        {
            var result = await _userService.Login(request);

            if (result.Status)
            {
                return new APIResult<UserLoginResponse>(result.Data);
            }
            else
            {
                return new APIResult<UserLoginResponse>(result.Error.GetErrorTypeByDescription<ProjectErrorCodes>().Value);
            }
        }
    }
}
