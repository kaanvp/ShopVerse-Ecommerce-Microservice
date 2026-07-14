using MediatR;
using ShopVerse.Order.Application.Interfaces;
using ShopVerse.Order.Domain.Enums;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Application.Commands.CancelOrderCommand
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<Unit>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Unit>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result<Unit>.Failure("Order not found.", 404);

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped)
                return Result<Unit>.Failure("Order cannot be cancelled at current status.", 400);

            order.UpdateStatus(OrderStatus.Cancelled);
            await _orderRepository.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
