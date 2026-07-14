using MediatR;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Application.Commands.CancelOrderCommand
{
    public class CancelOrderCommand : IRequest<Result<Unit>>
    {
        public Guid OrderId { get; set; }
    }
}
