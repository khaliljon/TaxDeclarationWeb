using System.ComponentModel.DataAnnotations;

namespace TaxDeclarationWeb.Models
{
    public class Inspection
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
    }
}