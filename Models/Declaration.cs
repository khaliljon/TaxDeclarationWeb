using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaxDeclarationWeb.Models;

[Table("Декларации")]
public class Declaration
{
    [Key]
    [Column("код_декларации")]
    [Display(Name = "Код декларации")]
    public int Id { get; set; }

    [Required]
    [Column("код_инспекции")]
    [Display(Name = "Инспекция")]
    public int InspectionId { get; set; }

    [ForeignKey("InspectionId")]
    [ValidateNever]
    public Inspection Inspection { get; set; }

    [Required]
    [Column("код_инспектора")]
    [Display(Name = "Инспектор")]
    public int InspectorId { get; set; }

    [ForeignKey("InspectorId")]
    [ValidateNever]
    public Inspector Inspector { get; set; }

    [Required]
    [Column("дата_подачи")]
    [Display(Name = "Дата подачи")]
    public DateTime SubmittedAt { get; set; }

    [Required]
    [Column("год")]
    [Display(Name = "Год")]
    public int Year { get; set; }

    [Required]
    [Column("ИИН")]
    [Display(Name = "ИИН плательщика")]
    public string TaxpayerIIN { get; set; }

    [ForeignKey("TaxpayerIIN")]
    [ValidateNever]
    public Taxpayer Taxpayer { get; set; }

    [Required]
    [Column("сумма_дохода")]
    [Display(Name = "Доход")]
    public decimal Income { get; set; }

    [Required]
    [Column("сумма_расхода")]
    [Display(Name = "Расходы")]
    public decimal Expenses { get; set; }

    [Required]
    [Column("необлагаемые_расходы")]
    [Display(Name = "Необлагаемые расходы")]
    public decimal NonTaxableExpenses { get; set; }

    [Required]
    [Column("сумма_уплаченных_налогов")]
    [Display(Name = "Уплаченные налоги")]
    public decimal PaidTaxes { get; set; }

    [NotMapped]
    [Display(Name = "Прибыль")]
    public decimal Profit => Income - Expenses;
}