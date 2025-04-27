using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Декларации")]
public class Declaration
{
    [Key]
    [Column("код_декларации")]
    public int Id { get; set; }

    [Column("код_инспекции")]
    public int InspectionId { get; set; }

    [ForeignKey("InspectionId")]
    public Inspection Inspection { get; set; }

    [Column("код_инспектора")]
    public int InspectorId { get; set; }

    [ForeignKey("InspectorId")]
    public Inspector Inspector { get; set; }

    [Column("дата_подачи")]
    public DateTime SubmittedAt { get; set; }

    [Column("год")]
    public int Year { get; set; }

    [Column("ИИН")]
    public string TaxpayerIIN { get; set; }

    [ForeignKey("TaxpayerIIN")]
    public Taxpayer Taxpayer { get; set; }

    [Column("сумма_дохода")]
    public decimal Income { get; set; }

    [Column("сумма_расхода")]
    public decimal Expenses { get; set; }

    [Column("необлагаемые_расходы")]
    public decimal NonTaxableExpenses { get; set; }

    [Column("сумма_уплаченных_налогов")]
    public decimal PaidTaxes { get; set; }

    [NotMapped]
    public decimal Profit => Income - Expenses;
}