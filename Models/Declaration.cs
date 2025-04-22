using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaxDeclarationWeb.Models;

[Table("Декларации")]
public class Declaration
{
    [Key]
    [Column("декларация")]
    public int Id { get; set; }

    [Column("инспекция")]
    public string InspectionId { get; set; }

    [ForeignKey("InspectionId")]
    public Inspection Inspection { get; set; }

    [Column("инспектор")]
    public string InspectorId { get; set; }

    [ForeignKey("InspectorId")]
    public Inspector Inspector { get; set; }

    [Column("подача")]
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