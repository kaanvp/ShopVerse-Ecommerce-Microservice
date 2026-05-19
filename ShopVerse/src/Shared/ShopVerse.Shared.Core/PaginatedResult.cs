using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Shared.Core
{
    /// Generic sayfalama sonucu temsil eden sarmalayıcı sınıftır.
    /// Veriyi sayfalanmış şekilde (Items) ve toplam kayıt sayısı, sayfa numarası ve sayfa boyutu gibi
    /// sayfalama metadataları ile birlikte döndürmek için kullanılır.
    /// </summary>
    public class PaginatedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public bool HasNextPage => (long)PageNumber * PageSize < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;

        private PaginatedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static PaginatedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize) =>
            new(items, totalCount, pageNumber, pageSize);
    }
}
