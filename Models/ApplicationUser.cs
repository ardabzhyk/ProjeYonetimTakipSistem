using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjeYonetimTakipSistem.Models
{
    /// <summary>
    /// Sistemdeki kullanıcıları temsil eden sınıf
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla {1} karakter olabilir.")]
        [Column("Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla {1} karakter olabilir.")]
        [Column("Soyad")]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();
        
        // Kullanıcının atandığı projeler
        public virtual ICollection<ProjeKullanici> ProjeKullanicilar { get; set; }
        
        // Kullanıcıya atanan görevler
        public virtual ICollection<Gorev> Gorevler { get; set; }

        public ApplicationUser()
        {
            ProjeKullanicilar = new HashSet<ProjeKullanici>();
            Gorevler = new HashSet<Gorev>();
        }
    }
} 