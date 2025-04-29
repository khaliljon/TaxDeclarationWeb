using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TaxDeclarationWeb.Models
{
    [Table("Налогоплательщики")]
    public class Taxpayer
    {
        [Key]
        [Column("ИИН")]
        [Display(Name = "ИИН")]
        public string IIN { get; set; }

        [Column("ФИО")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Column("адрес_проживания")]
        [Display(Name = "Адрес проживания")]
        public string Address { get; set; }

        [Column("телефон")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; }

        [Column("код_инспекции")]
        [Display(Name = "Инспекция")]
        public int InspectionCode { get; set; }

        [ForeignKey("InspectionCode")]
        [ValidateNever]
        public Inspection Inspection { get; set; }

        [Column("код_категории")]
        [Display(Name = "Категория")]
        public int CategoryCode { get; set; }

        [ForeignKey("CategoryCode")]
        [ValidateNever]
        public Category Category { get; set; }

        [Column("признак_обязательности_декларации")]
        [Display(Name = "Обязательная декларация")]
        public bool IsDeclarationRequired { get; set; }

        [Column("дата_рождения")]
        [Display(Name = "Дата рождения")]
        public DateTime BirthDate { get; set; }

        [Column("пол")]
        [Display(Name = "Пол")]
        public string Gender { get; set; }

        [Column("код_национальности")]
        [Display(Name = "Национальность")]
        public int NationalityCode { get; set; }

        [ForeignKey("NationalityCode")]
        [ValidateNever]
        public Nationality Nationality { get; set; }

        [Column("место_работы")]
        [Display(Name = "Место работы")]
        public string Workplace { get; set; }

        [Column("признак_резидентства")]
        [Display(Name = "Резидент")]
        public bool IsResident { get; set; }

        [Column("код_страны")]
        [Display(Name = "Страна")]
        public int CountryCode { get; set; }

        [ForeignKey("CountryCode")]
        [ValidateNever]
        public Country Country { get; set; }
    }
}
