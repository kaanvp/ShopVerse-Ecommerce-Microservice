using ShopVerse.Shared.Core;

namespace ShopVerse.Catalog.Domain.Entity
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
