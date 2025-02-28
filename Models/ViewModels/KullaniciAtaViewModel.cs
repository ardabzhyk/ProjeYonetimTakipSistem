using System.Collections.Generic;

namespace ProjeYonetimTakipSistem.Models.ViewModels
{
    public class KullaniciAtaViewModel
    {
        public int ProjeId { get; set; }
        public Proje Proje { get; set; } = null!;
        public IEnumerable<ApplicationUser> Kullanicilar { get; set; } = new List<ApplicationUser>();
    }
} 