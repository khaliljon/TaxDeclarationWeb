using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models
{
    public class Taxpayer
    {
        [Key]
        [StringLength(12)]
        public string IIN { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        public int InspectionId { get; set; }

        public int CategoryId { get; set; }

        public bool DeclarationRequired { get; set; }

        public DateTime BirthDate { get; set; }

        public string Gender { get; set; }

        public int NationalityId { get; set; }

        public string Workplace { get; set; }

        public string ResidencyStatus { get; set; } // "резидент" / "нерезидент"

        public int CountryId { get; set; }

        public Inspection Inspection { get; set; }

        public Category Category { get; set; }

        public Nationality Nationality { get; set; }

        public Country Country { get; set; }
    }
}