using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace TaxDeclarationWeb.Models
{
    public class Inspection
    {
        [Key]
        [Column("код_инспекции")]
        public int Code { get; set; }

        [Required(ErrorMessage = "Укажите наименование инспекции")]
        [Column("наименование")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Укажите адрес")]
        [Column("адрес")]
        public string Address { get; set; }

        [ValidateNever]
        public List<Inspector> Inspectors { get; set; }

        [ValidateNever]
        public List<Taxpayer> Taxpayers { get; set; }

        [ValidateNever]
        public List<Declaration> Declarations { get; set; }
    }
}
