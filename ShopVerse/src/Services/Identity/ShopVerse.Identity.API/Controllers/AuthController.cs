using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Identity.Application.Commands.ChangePassword;
using ShopVerse.Identity.Application.Commands.Login;
using ShopVerse.Identity.Application.Commands.RefreshToken;
using ShopVerse.Identity.Application.Commands.Register;
using ShopVerse.Identity.Application.Queries.GetCurrentUser;

namespace ShopVerse.Identity.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMediator _mediator;

        public AuthController(ILogger<AuthController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var result = await _mediator.Send(new GetCurrentUserQuery());
            return result.IsSuccess
           ? StatusCode(result.StatusCode, result.Data)
           : StatusCode(result.StatusCode, new { error = result.Error });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
            ? StatusCode(result.StatusCode, result.Data)
            : StatusCode(result.StatusCode, new { error = result.Error });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
            ? StatusCode(result.StatusCode, result.Data)
            : StatusCode(result.StatusCode, new { error = result.Error });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
            ? StatusCode(result.StatusCode, result.Data)
            : StatusCode(result.StatusCode, new { error = result.Error });
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess
            ? StatusCode(result.StatusCode, result.Data)
            : StatusCode(result.StatusCode, new { error = result.Error });
        }
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile()
        {
           await Task.Delay(0);
           throw new Exception("Not implemented");
        }
        

    }
}
