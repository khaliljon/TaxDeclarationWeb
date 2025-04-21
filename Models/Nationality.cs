using System.ComponentModel.DataAnnotations;

namespace TaxDeclarationWeb.Models
{
    public class Nationality
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}