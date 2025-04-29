using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaxDeclarationWeb.Models;

[Table("Национальности")]
public class Nationality
{
    [Key]
    [Column("код_национальности")]
    [Display(Name = "Код национальности")]
    public int Code { get; set; }

    [Required(ErrorMessage = "Укажите наименование национальности")]
    [Display(Name = "Наименование")]
    [Column("наименование")]
    public string Name { get; set; }

    [ValidateNever]
    public List<Taxpayer> Taxpayers { get; set; }
}