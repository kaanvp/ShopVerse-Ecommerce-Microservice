using MediatR;
using ShopVerse.Basket.Domain.Entity;
using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Basket.Application.Commands.AddToBasketCommand
{
    public class AddToBasketCommand : IRequest<Result<Unit>>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
