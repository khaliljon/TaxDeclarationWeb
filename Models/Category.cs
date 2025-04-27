using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Категории_плательщиков")]
public class Category
{
    [Key]
    [Column("код_категории")]
    public int Code { get; set; }

    [Column("наименование")]
    public string Name { get; set; }

    public List<Taxpayer> Taxpayers { get; set; }
}