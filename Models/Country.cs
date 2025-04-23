using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Страны")]
public class Country
{
    [Key]
    [Column("страна")]
    public string Code { get; set; }

    [Column("наименование")]
    public string Name { get; set; }

    public List<Taxpayer> Taxpayers { get; set; }
}