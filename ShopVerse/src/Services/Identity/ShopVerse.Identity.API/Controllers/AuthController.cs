using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShopVerse.Identity.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpGet("/me")]
        public async Task<Guid> GetCurrentUser()
        {
            return new Guid();
        }
        [HttpPost("/login")]
        public async Task<Guid> Login()
        {
            return new Guid();
        }
        [HttpPost("/register")]
        public async Task<Guid> Register()
        {
            return new Guid();
        }
        [HttpPost("/refresh-token")]
        public async Task<Guid> RefreshToken()
        {
            return new Guid();
        }
        [HttpPost("/change-password")]
        public async Task<Guid> ChangePassword()
        {
            return new Guid();
        }
        [HttpPut("/profile")]
        public async Task<Guid> UpdateProfile()
        {
            return new Guid();
        }
        

    }
}
