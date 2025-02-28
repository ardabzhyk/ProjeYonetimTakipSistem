using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjeYonetimTakipSistem.Models
{
    /// <summary>
    /// Proje ve kullanıcı arasındaki çoka-çok ilişkiyi temsil eden sınıf
    /// </summary>
    public class ProjeKullanici
    {
        public int Id { get; set; }

        [Required]
        public int ProjeId { get; set; }

        [Required]
        public virtual Proje Proje { get; set; } = null!;

        [Required]
        public string KullaniciId { get; set; } = string.Empty;

        [Required]
        public virtual ApplicationUser Kullanici { get; set; } = null!;

        [DataType(DataType.Date)]
        public DateTime AtamaTarihi { get; set; } = DateTime.Now;
    }
} 