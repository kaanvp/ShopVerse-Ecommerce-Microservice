using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Basket.Application.DTOs
{
    public class BasketDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<BasketItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }
}
