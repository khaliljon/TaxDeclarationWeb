using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Национальности")]
public class Nationality
{
    [Key]
    [Column("национальность")]
    public string Code { get; set; }

    [Column("наименование")]
    public string Name { get; set; }

    public List<Taxpayer> Taxpayers { get; set; }
}