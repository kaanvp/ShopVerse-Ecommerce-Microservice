using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopVerse.Identity.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Infrastructure.Context
{
    public class IdentityDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Gerekirse ek konfigürasyonlar buraya eklenebilir
            // Örn: Tablo isimlerini özelleştirme veya index ekleme
        }
    }
}
