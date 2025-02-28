using System.ComponentModel.DataAnnotations;

namespace ProjeYonetimTakipSistem.Models
{
    /// <summary>
    /// Sistemdeki projeleri temsil eden sınıf
    /// </summary>
    public class Proje
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Proje adı zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en fazla {1} karakter olmalıdır.")]
        [Display(Name = "Proje Adı")]
        public string Ad { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Başlangıç Tarihi")]
        public DateTime BaslangicTarihi { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Bitiş Tarihi")]
        public DateTime BitisTarihi { get; set; }

        [Display(Name = "Durum")]
        public ProjeDurumu Durum { get; set; }

        // İlişkiler
        public virtual ICollection<ProjeKullanici> ProjeKullanicilari { get; set; } = new List<ProjeKullanici>();
        public virtual ICollection<Gorev> Gorevler { get; set; } = new List<Gorev>();

        public Proje()
        {
            ProjeKullanicilari = new HashSet<ProjeKullanici>();
            Gorevler = new HashSet<Gorev>();
        }
    }

    public enum ProjeDurumu
    {
        [Display(Name = "Başlamadı")]
        Baslamadi = 0,
        [Display(Name = "Devam Ediyor")]
        DevamEdiyor = 1,
        [Display(Name = "Tamamlandı")]
        Tamamlandi = 2,
        [Display(Name = "İptal Edildi")]
        IptalEdildi = 3
    }
} 