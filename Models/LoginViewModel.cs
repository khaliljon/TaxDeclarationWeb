﻿using System.ComponentModel.DataAnnotations;

namespace TaxDeclarationWeb.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Логин (email)")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}