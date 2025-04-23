using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Инспекторы")]
public class Inspector
{
    [Key]
    [Column("инспектор")]
    public string Code { get; set; }

    [Column("ФИО")]
    public string FullName { get; set; }

    [Column("код_инспекции")]
    public string InspectionCode { get; set; }

    [ForeignKey("InspectionCode")]
    public Inspection Inspection { get; set; }

    [Column("телефон")]
    public string Phone { get; set; }

    public List<Declaration> Declarations { get; set; }
}