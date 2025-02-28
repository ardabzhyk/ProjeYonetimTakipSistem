using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjeYonetimTakipSistem.Models.ViewModels
{
    public class KullaniciListViewModel
    {
        public required string Id { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public required string LastName { get; set; }

        public List<string> Roles { get; set; } = new();
    }

    public class KullaniciDuzenleViewModel : KullaniciListViewModel
    {
        [Display(Name = "Mevcut Roller")]
        public List<string> CurrentRoles { get; set; } = new List<string>();

        [Display(Name = "Tüm Roller")]
        public List<string> AllRoles { get; set; } = new List<string>();

        [Display(Name = "Seçili Roller")]
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }

    public class KullaniciOlusturViewModel
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public required string ConfirmPassword { get; set; }

        [Display(Name = "Tüm Roller")]
        public List<string> AllRoles { get; set; } = new List<string>();

        [Display(Name = "Seçili Roller")]
        public List<string> SelectedRoles { get; set; } = new() { "User" };
    }
} 