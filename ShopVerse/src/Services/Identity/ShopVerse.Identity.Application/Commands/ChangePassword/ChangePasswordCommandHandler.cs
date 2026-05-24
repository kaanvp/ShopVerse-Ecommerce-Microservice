using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ShopVerse.Identity.Application.DTOs;
using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
    {
        private readonly ILogger<ChangePasswordCommandHandler> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangePasswordCommandHandler(ILogger<ChangePasswordCommandHandler> logger, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated when attempting to change password.");
                return Result<bool>.Failure("User not authenticated", StatusCodes.Status401Unauthorized, cancellationToken);
            }    
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result<bool>.Failure("User not founded", StatusCodes.Status401Unauthorized, cancellationToken);
            }
            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword,request.NewPassword);
            if(!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Failed to change password for user {UserId}. Errors: {Errors}", userId, errors);
                return Result<bool>.Failure(errors, StatusCodes.Status400BadRequest, cancellationToken);
            }
            return Result<bool>.Success(true);
        }
    }
}
