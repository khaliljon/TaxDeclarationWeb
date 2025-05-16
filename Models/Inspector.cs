using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaxDeclarationWeb.Models;

[Table("Инспекторы")]
public class Inspector
{
    [Key]
    [Column("код_инспектора")]
    public int Code { get; set; }

    [ValidateNever]
    [Column("user_id")]
    public string UserId { get; set; }

    [Required(ErrorMessage = "Укажите ФИО")]
    [Display(Name = "ФИО")]
    [Column("ФИО")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Выберите инспекцию")]
    [Display(Name = "Инспекция")]
    [Column("код_инспекции")]
    public int InspectionCode { get; set; }

    [ForeignKey("InspectionCode")]
    [ValidateNever] 
    public Inspection Inspection { get; set; }

    [Required(ErrorMessage = "Укажите номер телефона")]
    [Display(Name = "Телефон")]
    [Column("телефон")]
    public string Phone { get; set; }

    [ValidateNever]
    public List<Declaration> Declarations { get; set; }
}