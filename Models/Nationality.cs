using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Национальности")]
public class Nationality
{
    [Key]
    [Column("код_национальности")]
    public int Code { get; set; }

    [Column("наименование")]
    public string Name { get; set; }

    public List<Taxpayer> Taxpayers { get; set; }
}