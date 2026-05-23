using MediatR;
using ShopVerse.Identity.Application.DTOs;
using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.Commands.Register
{
    public record RegisterCommand(string FirstName, string LastName, string Email, string Password) : IRequest<Result<AuthResponseDto>>;
}
