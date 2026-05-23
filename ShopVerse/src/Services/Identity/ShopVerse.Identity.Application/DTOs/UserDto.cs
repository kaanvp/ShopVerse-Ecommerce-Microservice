using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.DTOs
{
    public record UserDto
    {
        public Guid Id { get; init; }
        public string UserName { get; init; }
        public string Email { get; init; }
        IList<string> Roles { get; init; }
    }
}
