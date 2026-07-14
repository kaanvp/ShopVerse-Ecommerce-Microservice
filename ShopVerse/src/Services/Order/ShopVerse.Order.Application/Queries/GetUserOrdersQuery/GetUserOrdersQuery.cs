using MediatR;
using ShopVerse.Order.Application.DTOs;
using ShopVerse.Shared.Core;

namespace ShopVerse.Order.Application.Queries.GetUserOrdersQuery
{
    public class GetUserOrdersQuery : IRequest<Result<List<OrderDto>>>
    {
        // Kullanıcı ID'si JWT'den alınacak, query parametre gerekmez
    }
}
