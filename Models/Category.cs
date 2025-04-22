using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Категории_плательщиков")]
public class Category
{
    [Key]
    [Column("категория")]
    public string Code { get; set; }

    [Column("наименование")]
    public string Name { get; set; }
}