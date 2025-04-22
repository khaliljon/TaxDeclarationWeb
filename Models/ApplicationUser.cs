using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

public class ApplicationUser : IdentityUser
{
    public string? InspectorId { get; set; }

    [ForeignKey("InspectorId")]
    public Inspector? Inspector { get; set; }

    public string Role { get; set; } = "Taxpayer";
}