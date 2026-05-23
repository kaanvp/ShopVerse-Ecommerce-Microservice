using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.DTOs
{
    public record AuthResponseDto
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public UserDto User { get; init; }
    }
}
