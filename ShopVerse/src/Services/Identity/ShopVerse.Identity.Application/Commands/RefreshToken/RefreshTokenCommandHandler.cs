using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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

namespace ShopVerse.Identity.Application.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(ITokenService tokenService, UserManager<AppUser> userManager, ILogger<RefreshTokenCommandHandler> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. Süresi dolmuş access token'dan kullanıcı bilgilerini (principal) çıkar
            var principal = _tokenService.GetClaimsPrincipalFromExpiredToken(request.AccessToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Result<AuthResponseDto>.Failure("Geçersiz token.", 401);

            // 2. Kullanıcıyı ve Refresh Token eşleşmesini kontrol et
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                _logger.LogWarning("Invalid or expired refresh token. UserId: {UserId}", userId);
                return Result<AuthResponseDto>.Failure("Invalid or expired refresh token", 401);
            }
            // 3. Yeni token çiftini üret
            var newAccessToken = _tokenService.GenerateAccesToken(principal);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // 4. Yeni refresh token'ı veritabanına kaydet (7 gün)
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            // 5. DTO oluştur ve başarılı sonucu dön
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto(user.Id, $"{user.FirstName} {user.LastName}", user.Email!, roles);
            var response = new AuthResponseDto(newAccessToken, newRefreshToken, userDto);
            return Result<AuthResponseDto>.Success(response, 200);
        }
    }
}
