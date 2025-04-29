using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaxDeclarationWeb.Models;

[Table("Налоговые_инспекции")]
public class Inspection
{
    [Key]
    [Required(ErrorMessage = "Укажите код инспекции")]
    [Display(Name = "Код инспекции")]
    [Column("код_инспекции")]
    public int Code { get; set; }

    [Required(ErrorMessage = "Укажите наименование инспекции")]
    [Display(Name = "Наименование")]
    [Column("наименование")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Укажите адрес")]
    [Display(Name = "Адрес")]
    [Column("адрес")]
    public string Address { get; set; }

    [ValidateNever]
    public List<Inspector> Inspectors { get; set; }

    [ValidateNever]
    public List<Taxpayer> Taxpayers { get; set; }

    [ValidateNever]
    public List<Declaration> Declarations { get; set; }
}