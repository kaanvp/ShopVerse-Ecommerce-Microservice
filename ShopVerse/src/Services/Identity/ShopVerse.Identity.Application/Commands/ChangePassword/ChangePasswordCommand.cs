using MediatR;
using ShopVerse.Identity.Application.DTOs;
using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.Commands.ChangePassword
{
    public record ChangePasswordCommand(
        string UserId,
        string CurrentPassword,
        string NewPassword
    ) : IRequest<Result<bool>>;

}
