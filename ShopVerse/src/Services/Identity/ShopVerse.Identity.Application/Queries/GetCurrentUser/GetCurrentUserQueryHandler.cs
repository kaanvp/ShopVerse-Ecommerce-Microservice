using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ShopVerse.Identity.Application.DTOs;
using ShopVerse.Identity.Application.Services;
using ShopVerse.Identity.Domain.Entity;
using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.Queries.GetCurrentUser
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GetCurrentUserQueryHandler(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            // 1. JWT token'dan userId claim'ini oku
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Result<UserDto>.Failure("User not authenticated", 401);
            }

            // 2. Kullanıcıyı veritabanından çek
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return Result<UserDto>.Failure("User not found", 404);
            }

            // 3. Rolleri al ve DTO oluştur
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto(user.Id,$"{user.FirstName} {user.LastName}",user.Email!,roles);

            return Result<UserDto>.Success(userDto);    
        }
    }
}
