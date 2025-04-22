using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}