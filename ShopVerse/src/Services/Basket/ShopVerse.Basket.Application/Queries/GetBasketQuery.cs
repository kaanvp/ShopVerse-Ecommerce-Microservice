using MediatR;
using ShopVerse.Basket.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Basket.Application.Queries
{
    public class GetBasketQuery : IRequest<Result<BasketDto>>
    {
    }
}
