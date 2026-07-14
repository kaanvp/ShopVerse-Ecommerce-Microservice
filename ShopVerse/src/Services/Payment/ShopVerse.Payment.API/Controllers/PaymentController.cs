using Microsoft.AspNetCore.Mvc;
using ShopVerse.Payment.Application.Interfaces;

namespace ShopVerse.Payment.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        /// <summary>
        /// Sipariş ID'sine göre ödeme durumunu sorgular.
        /// </summary>
        [HttpGet("order/{orderId:guid}")]
        public async Task<IActionResult> GetByOrderId(Guid orderId)
        {
            var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (payment is null)
                return NotFound(new { Message = $"No payment found for OrderId: {orderId}" });

            return Ok(new
            {
                payment.Id,
                payment.OrderId,
                payment.Amount,
                Status = payment.Status.ToString(),
                payment.TransactionId,
                payment.CreatedAt
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment is null) return NotFound();
            return Ok(payment);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentRepository.GetAllAsync();
            var result = payments.Select(p => new
            {
                p.Id,
                p.OrderId,
                p.Amount,
                Status = p.Status.ToString(),
                p.TransactionId,
                p.CreatedAt
            });
            return Ok(result);
        }
    }
}
