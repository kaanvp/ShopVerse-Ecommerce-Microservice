using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ShopVerse.Identity.Application.DTOs;
using ShopVerse.Identity.Application.Services;
using ShopVerse.Identity.Domain.Entity;
using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IdentityDbContext _context;
        public LoginCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService, IdentityDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }
        public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {

            // 1. Kullanıcıyı bul
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Result<AuthResponseDto>.Failure("Invalid email or password", 401);
            }

            // 2. Şifreyi Kontrol et
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return Result<AuthResponseDto>.Failure("Invalid email or password", 401);
            }

            // 3. Rolleri Al
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // 4. Tokenlerı Oluştur
            var accessToken = _tokenService.GenerateAccesToken(claimsPrincipal);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 5. Refresh Token'ı Veritabanına Kaydet
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Refresh token 7 gün geçerli olsun  
            await _userManager.UpdateAsync(user);

            // 6. Cevap DTO'sunu Oluştur
            var userDto = new UserDto(user.Id, $"{user.FirstName} {user.LastName}", user.Email!, roles);
            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userDto
            };
            return Result<AuthResponseDto>.Success(response, 200);

        }
    }
}
