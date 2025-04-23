using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Налоговые_инспекции")]
public class Inspection
{
    [Key]
    [Column("инспекция")]
    public string Code { get; set; }

    [Column("наименование")]
    public string Name { get; set; }

    [Column("адрес")]
    public string Address { get; set; }

    public List<Inspector> Inspectors { get; set; }
    public List<Taxpayer> Taxpayers { get; set; }
    public List<Declaration> Declarations { get; set; }
}