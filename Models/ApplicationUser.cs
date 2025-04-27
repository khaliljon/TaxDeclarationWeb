using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

public class ApplicationUser : IdentityUser
{
    public string? IIN { get; set; }   // <--- Вот это ключевое для связи с Taxpayer!

    public int? InspectorId { get; set; }

    [ForeignKey("InspectorId")]
    public Inspector? Inspector { get; set; }
}