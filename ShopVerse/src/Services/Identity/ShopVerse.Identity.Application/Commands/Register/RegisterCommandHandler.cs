using MediatR;
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

namespace ShopVerse.Identity.Application.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        public RegisterCommandHandler(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. Kullanıcının daha önce kayıtlı olup olmadığını kontrol et
            var existingUser = _userManager.FindByEmailAsync(request.Email);
            if(existingUser != null)
            {
                return Result<AuthResponseDto>.Failure("Email is already registered.", 400);
            }

            // 2. Yeni kullanıcıyı oluştur
            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                Email = request.Email,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<AuthResponseDto>.Failure($"User creation failed: {errors}", 400);
            }

            // 3. Customer rolünü ata
            await _userManager.AddToRoleAsync(user, "Customer");
            var roles = new List<string> { "Customer" };

            // 4. Token üretimi için Claims oluştur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // 5. Access ve Refresh token üret
            var accessToken = _tokenService.GenerateAccesToken(principal);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 6. RefreshToken'ı DB'ye kaydet
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Refresh token 7 gün geçerli olsun
            await _userManager.UpdateAsync(user);

            // 7. DTO'ları oluştur ve başarılı yanıtı dön (201 Created)
            var userDto = new UserDto(user.Id, $"{user.FirstName} {user.LastName}", user.Email!, roles);
            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userDto
            };
            return Result<AuthResponseDto>.Success(response, 201);
        }
    }
}
