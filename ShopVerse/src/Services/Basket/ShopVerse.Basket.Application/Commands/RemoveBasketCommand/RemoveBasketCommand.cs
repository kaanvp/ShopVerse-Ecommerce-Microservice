using MediatR;
using ShopVerse.Shared.Core;

namespace ShopVerse.Basket.Application.Commands.RemoveBasketCommand
{
    public class RemoveBasketCommand : IRequest<Result<Unit>>
    {
    }
}
