using MediatR;
using ShopVerse.Identity.Application.DTOs;
using ShopVerse.Identity.Domain.Entity;
using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.Commands.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
}
