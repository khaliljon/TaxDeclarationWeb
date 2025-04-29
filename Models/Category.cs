using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaxDeclarationWeb.Models;

[Table("Категории_плательщиков")]
public class Category
{
    [Key]
    [Column("код_категории")]
    [Display(Name = "Код категории")]
    public int Code { get; set; }

    [Required(ErrorMessage = "Укажите наименование категории")]
    [Display(Name = "Наименование")]
    [Column("наименование")]
    public string Name { get; set; }

    [ValidateNever]
    public List<Taxpayer> Taxpayers { get; set; }
}