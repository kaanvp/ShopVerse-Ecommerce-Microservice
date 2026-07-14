using ShopVerse.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Basket.Domain.Entity
{
    public class BasketCart
    {
        public string UserId { get; set; } = string.Empty;
        public List<BasketItem> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);

        public void AddItem(BasketItem item)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveItem(Guid productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}
