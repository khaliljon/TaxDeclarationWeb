using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models
{
    public class Inspector
    {
        [Key]
        public int Id { get; set; }

        public string FullName { get; set; }

        [ForeignKey("Inspection")]
        public int InspectionId { get; set; }

        public string Phone { get; set; }

        public Inspection Inspection { get; set; }
    }
}