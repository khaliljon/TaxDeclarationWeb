using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Страны")]
public class Country
{
    [Key]
    [Column("страна")]
    public string Code { get; set; }

    [Column("наименование")]
    public string Name { get; set; }
}