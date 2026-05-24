using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopVerse.Identity.Application.DTOs
{
    public record ChangePasswordDto(string CurrentPassword,string NewPassword);
}
