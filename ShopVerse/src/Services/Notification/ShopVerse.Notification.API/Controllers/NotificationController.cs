using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Notification.Application.Interfaces;
using ShopVerse.Shared.Core;
using System.Security.Claims;

namespace ShopVerse.Notification.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Kullanıcının tüm bildirimlerini getirir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            var result = notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt,
                n.ReadAt
            });
            return Ok(result);
        }

        /// <summary>
        /// Bildirimi okundu olarak işaretler.
        /// </summary>
        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification is null)
                return NotFound();

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
