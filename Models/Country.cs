using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaxDeclarationWeb.Models;

[Table("Страны")]
public class Country
{
    [Key]
    [Column("код_страны")]
    [Display(Name = "Код страны")]
    public int Code { get; set; }

    [Required(ErrorMessage = "Укажите название страны")]
    [Column("наименование")]
    [Display(Name = "Наименование")]
    public string Name { get; set; }

    [ValidateNever]
    public List<Taxpayer> Taxpayers { get; set; }
}