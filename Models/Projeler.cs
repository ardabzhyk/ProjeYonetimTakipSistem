using System;
using System.Collections.Generic;

namespace ProjeYonetimTakipSistem.Models;

public partial class Projeler
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public string? Aciklama { get; set; }

    public DateTime BaslangicTarihi { get; set; }

    public DateTime BitisTarihi { get; set; }

    public int Durum { get; set; }

    public virtual ICollection<Gorevler> Gorevlers { get; set; } = new List<Gorevler>();

    public virtual ICollection<ProjeKullanicilar> ProjeKullanicilars { get; set; } = new List<ProjeKullanicilar>();
}
