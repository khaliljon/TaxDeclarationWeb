using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaxDeclarationWeb.Models;

[Table("Налогоплательщики")]
public class Taxpayer
{
    [Key]
    [Column("ИИН")]
    public string IIN { get; set; }

    [Column("ФИО")]
    public string FullName { get; set; }

    [Column("адрес_проживания")]
    public string Address { get; set; }

    [Column("телефон")]
    public string Phone { get; set; }

    [Column("код_инспекции")]
    public string InspectionCode { get; set; }

    [ForeignKey("InspectionCode")]
    public Inspection Inspection { get; set; }

    [Column("код_категории")]
    public string CategoryCode { get; set; }

    [ForeignKey("CategoryCode")]
    public Category Category { get; set; }

    [Column("признак_обязательности_декларации")]
    public bool IsDeclarationRequired { get; set; }

    [Column("дата_рождения")]
    public DateTime BirthDate { get; set; }

    [Column("пол")]
    public string Gender { get; set; }

    [Column("код_национальности")]
    public string NationalityCode { get; set; }

    [ForeignKey("NationalityCode")]
    public Nationality Nationality { get; set; }

    [Column("место_работы")]
    public string Workplace { get; set; }

    [Column("признак_резидентства")]
    public bool IsResident { get; set; }

    [Column("код_страны")]
    public string CountryCode { get; set; }

    [ForeignKey("CountryCode")]
    public Country Country { get; set; }
}