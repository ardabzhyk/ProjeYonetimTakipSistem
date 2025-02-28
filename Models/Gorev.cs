using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjeYonetimTakipSistem.Models
{
    /// <summary>
    /// Projelerdeki görevleri temsil eden sınıf
    /// </summary>
    public class Gorev
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Görev adı zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en fazla {1} karakter olmalıdır.")]
        [Display(Name = "Görev Adı")]
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

        [Required(ErrorMessage = "Öncelik seçimi zorunludur.")]
        [Display(Name = "Öncelik")]
        public GorevOnceligi Oncelik { get; set; } = GorevOnceligi.Orta;

        [Required(ErrorMessage = "Durum seçimi zorunludur.")]
        [Display(Name = "Durum")]
        public GorevDurumu Durum { get; set; }

        // İlişkiler
        [Required(ErrorMessage = "Proje seçimi zorunludur.")]
        [Display(Name = "Proje")]
        public int ProjeId { get; set; }
        
        [ForeignKey("ProjeId")]
        public virtual Proje Proje { get; set; } = null!;

        [Required(ErrorMessage = "Görev atanacak kullanıcı seçimi zorunludur.")]
        [Display(Name = "Atanan Kullanıcı")]
        public string AtananKullaniciId { get; set; } = string.Empty;
        
        [ForeignKey("AtananKullaniciId")]
        public virtual ApplicationUser AtananKullanici { get; set; } = null!;
    }

    public enum GorevOnceligi
    {
        [Display(Name = "Düşük")]
        Dusuk = 0,
        [Display(Name = "Orta")]
        Orta = 1,
        [Display(Name = "Yüksek")]
        Yuksek = 2
    }

    public enum GorevDurumu
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