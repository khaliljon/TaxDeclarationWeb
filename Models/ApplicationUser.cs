using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Уникальный идентификатор в системе (например, ИИН)
        public string IIN { get; set; }


    }
}